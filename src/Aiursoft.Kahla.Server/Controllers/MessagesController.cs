using System.Security.Cryptography;
using Aiursoft.AiurObserver;
using Aiursoft.AiurObserver.WebSocket.Server;
using Aiursoft.AiurProtocol.Models;
using Aiursoft.AiurProtocol.Server.Attributes;
using Aiursoft.Kahla.Server.Attributes;
using Aiursoft.Kahla.Server.Data;
using Microsoft.AspNetCore.Mvc;
using Aiursoft.AiurProtocol.Exceptions;
using Aiursoft.AiurProtocol.Server;
using Aiursoft.ArrayDb.Partitions;
using Aiursoft.Kahla.Entities;
using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.SDK.Models.ViewModels;
using Aiursoft.Kahla.SDK.Services;
using Aiursoft.Kahla.Server.Models;
using Aiursoft.Kahla.Server.Services.Messages;
using Aiursoft.WebTools.Attributes;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Controllers;

[LimitPerMin]
[ApiExceptionHandler(
    PassthroughRemoteErrors = true,
    PassthroughAiurServerException = true)]
[ApiModelStateChecker]
[Route("api/messages")]
public class MessagesController(
    ChannelMessageService channelMessageService,
    QuickMessageAccess quickMessageAccess,
    LocksInMemoryDb locksInMemory,
    PartitionedObjectBucket<MessageInDatabaseEntity, int> messages,
    IDataProtectionProvider dataProtectionProvider,
    ChannelsInMemoryDb memoryDb,
    KahlaRelationalDbContext relationalDbContext,
    ILogger<MessagesController> logger) : ControllerBase
{
    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector("WebSocketOTP");
    public static TimeSpan TokenTimeout = TimeSpan.FromMinutes(5);

    [HttpPost]
    [KahlaForceAuth]
    [Route("init-websocket")]
    public IActionResult InitWebSocket()
    {
        var userId = User.GetUserId();
        logger.LogInformation("User with Id: {Id} is trying to init a websocket OTP.", userId);
        var validTo = DateTime.UtcNow.Add(TokenTimeout);
        var otpRaw = $"uid={userId},vlt={validTo}";
        var protectedOtp = _protector.Protect(otpRaw);
        return this.Protocol(new InitPusherViewModel
        {
            Code = Code.ResultShown,
            Message = "Successfully generated a new OTP. It will be valid for 5 minutes.",
            WebSocketEndpoint =
                $"{HttpContext.Request.Scheme.Replace("http", "ws")}://{HttpContext.Request.Host}/api/messages/websocket/{userId}?otp={protectedOtp}"
        });
    }
    
    [HttpPost]
    [KahlaForceAuth]
    [Route("init-thread-websocket/{id}")]
    public async Task<IActionResult> InitWebSocketForThread([FromRoute] int id)
    {
        var currentUserId = User.GetUserId();
        logger.LogInformation("User with Id: {Id} is trying to init a channel websocket OTP. Thread ID: {ThreadId}", currentUserId, id);
        var thread = await relationalDbContext.ChatThreads.FindAsync(id);
        if (thread == null)
        {
            return this.Protocol(Code.NotFound, "The thread does not exist.");
        }

        var myRelation = await relationalDbContext.UserThreadRelations
            .Where(t => t.UserId == currentUserId)
            .Where(t => t.ThreadId == id)
            .FirstOrDefaultAsync();
        if (myRelation == null)
        {
            return this.Protocol(Code.Unauthorized, "You are not a member of this thread.");
        }

        var validTo = DateTime.UtcNow.Add(TokenTimeout);
        var otpRaw = $"uid={currentUserId},vlt={validTo},tid={id}";
        var protectedOtp = _protector.Protect(otpRaw);
        return this.Protocol(new InitPusherViewModel
        {
            Code = Code.ResultShown,
            Message = $"Successfully generated a new OTP for thread with id: '{id}'. It will be valid for 5 minutes.",
            WebSocketEndpoint =
                $"{HttpContext.Request.Scheme.Replace("http", "ws")}://{HttpContext.Request.Host}/api/messages/websocket-thread/{id}/{currentUserId}/{protectedOtp}"
        });
    }

    [EnforceWebSocket]
    [Route("websocket/{userId}")]
    public async Task<IActionResult> WebSocket(
        [FromRoute] string userId, 
        [FromQuery] string otp)
    {
        try
        {
            var otpRaw = _protector.Unprotect(otp);
            var parts = otpRaw.Split(',');
            var userIdInOtp = parts[0].Split('=')[1];
            var validTo = DateTime.Parse(parts[1].Split('=')[1]);
            if (userIdInOtp != userId)
            {
                logger.LogWarning("User with ID: {UserId} is trying to get a websocket with invalid OTP {HisOTP}.",
                    userId, otp);
                return this.Protocol(Code.Unauthorized, "Invalid OTP.");
            }

            if (validTo < DateTime.UtcNow)
            {
                logger.LogWarning("User with ID: {UserId} is trying to get a websocket with expired OTP {HisOTP}.",
                    userId, otp);
                return this.Protocol(Code.Unauthorized, "Expired OTP.");
            }
        }
        catch (CryptographicException)
        {
            logger.LogWarning("User with ID: {UserId} is trying to get a websocket with invalid OTP {HisOTP}.",
                userId, otp);
            return this.Protocol(Code.Unauthorized, "Invalid OTP.");
        }

        logger.LogInformation("User with Id: {Id} is trying to get a websocket. And he provided the correct OTP.",
            userId);
        var pusher = await HttpContext.AcceptWebSocketClient();
        var channel = memoryDb.GetUserChannel(userId);
        var sub = channel.Subscribe(t => pusher.Send(t, HttpContext.RequestAborted));

        try
        {
            await pusher.Listen(HttpContext.RequestAborted);
        }
        finally
        {
            sub.Unsubscribe();
            logger.LogInformation("User with Id: {Id} disconnected from the user channel websocket.", userId);
        }

        return new EmptyResult();
    }

    [EnforceWebSocket]
    [Route("websocket-thread/{threadId:int}/{userId}/{otp}")]
    public async Task<IActionResult> WebSocketForThread(
        [FromRoute] int threadId,
        [FromRoute] string userId,
        [FromRoute] string otp,
        [FromQuery] int? start)
    {
        var threadCache = quickMessageAccess.GetThreadCache(threadId);
        EnsureUserIsMemberOfThread(threadId, userId, otp, threadCache);
        var messagesDb = messages.GetPartitionById(threadId);
        var threadReflector = memoryDb.GetThreadChannel(threadId);
        var threadMessagesLock = locksInMemory.GetThreadMessagesLock(threadId);
        var user = await relationalDbContext.Users.FindAsync(userId);

        logger.LogInformation("User with ID: {UserId} is trying to connect to thread {ThreadId}.", userId, threadId);
        var socket = await HttpContext.AcceptWebSocketClient();
        logger.LogInformation("User with ID: {UserId} is accepted to connect to thread {ThreadId}.", userId, threadId);
        
        var clientPushConsumer = new ClientPushConsumer(
            channelMessageService,
            new KahlaUserMappedPublicView
            {
                Id = user!.Id,
                NickName = user.NickName,
                Bio = user.Bio,
                IconFilePath = user.IconFilePath,
                AccountCreateTime = user.AccountCreateTime,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed 
            },
            threadId,
            logger);
        var reflectorConsumer = new ThreadReflectConsumer(
            threadCache,
            userId,
            logger,
            socket);

        // Initial pull. (Lock read, avoid new messages during this time.)
        logger.LogInformation("User with ID: {UserId} is trying to get initial pull length from thread {ThreadId}.", userId, threadId);
        threadMessagesLock.EnterReadLock();
        try
        {
            var startLocation = start ?? 0;
            // Example: Totally 30 messages. User start from 20, then he should read 10 messages.
            var readLength = messagesDb.Count - startLocation;
            if (readLength > 0)
            {
                logger.LogInformation(
                    "User with ID: {UserId} is trying to pull initial {ReadLength} messages from thread {ThreadId}. Start Index is {StartIndex}",
                    userId, readLength, threadId, startLocation);
                var firstPullRequest = messagesDb.ReadBulk(startLocation, readLength);
                
                // Do NOT await here, because the read write lock need the same thread to release the lock.
                socket.Send(SDK.Extensions.Serialize(firstPullRequest.Select(t => t.ToCommit()))).Wait();
            }

        }
        finally
        {
            threadMessagesLock.ExitReadLock();
        }
        
        // Clear current user's unread message count and at status.
        threadCache.ClearUserUnReadAmount(userId);
        threadCache.ClearAtForUser(userId);
        
        // Configure the reflector and the client push consumer.
        var refSub = threadReflector.Subscribe(reflectorConsumer);
        var socSub = socket.Subscribe(clientPushConsumer);
        logger.LogInformation("User with ID: {UserId} finished initial push and connected to thread {ThreadId} and listening for new events.",
            userId, threadId);

        try
        {
            // Start listening.
            await socket.Listen(HttpContext.RequestAborted);
        }
        finally
        {
            // Unsubscribe.
            refSub.Unsubscribe();
            socSub.Unsubscribe();
            logger.LogInformation("User with ID: {UserId} disconnected from thread {ThreadId} channel websocket.", userId, threadId);
        }

        return new EmptyResult();
    }

    [HttpPost]
    [KahlaForceAuth]
    [Route("direct-send/{threadId:int}")]
    public async Task<IActionResult> DirectSend(
        [FromRoute] int threadId,
        [FromBody] Commit<ChatMessage> message)
    {
        var currentUserId = User.GetUserId();
        logger.LogInformation("User with Id: {Id} is trying to directly send a message to thread {ThreadId}.", currentUserId, threadId);
        var thread = await relationalDbContext.ChatThreads.FindAsync(threadId);
        if (thread == null)
        {
            return this.Protocol(Code.NotFound, "The thread does not exist.");
        }

        var myRelation = await relationalDbContext.UserThreadRelations
            .Where(t => t.UserId == currentUserId)
            .Where(t => t.ThreadId == threadId)
            .FirstOrDefaultAsync();
        if (myRelation == null)
        {
            return this.Protocol(Code.Unauthorized, "You are not a member of this thread.");
        }
        
        var user = await relationalDbContext.Users.FindAsync(currentUserId);
        var userView = new KahlaUserMappedPublicView
        {
            Id = user!.Id,
            NickName = user.NickName,
            Bio = user.Bio,
            IconFilePath = user.IconFilePath,
            AccountCreateTime = user.AccountCreateTime,
            Email = user.Email,
            EmailConfirmed = user.EmailConfirmed 
        };
        await channelMessageService.SendMessagesToChannel([message], threadId, userView);
        
        var unRead = quickMessageAccess.GetThreadCache(threadId).GetUserUnReadAmount(currentUserId);
        if (unRead == 1)
        {
            // After direct send, the newly sent message was the only unread message.
            // So we should clear the at status for the user.
            quickMessageAccess.GetThreadCache(threadId).ClearAtForUser(currentUserId);
            quickMessageAccess.GetThreadCache(threadId).ClearUserUnReadAmount(currentUserId);
        }
        return this.Protocol(Code.JobDone, "Successfully pushed the message.");
    }

    // TODO: This function should be migrated to a service. Should have an arg: LowPerformance from db to judge. HighPerformance from memory dictionary to judge.
    private void EnsureUserIsMemberOfThread(int threadId, string userId, string otp, ThreadStatusInMemoryCache threadStatuCache)
    {
        try
        {
            var otpRaw = _protector.Unprotect(otp);
            var parts = otpRaw.Split(',');
            var userIdInOtp = parts[0].Split('=')[1];
            var validTo = DateTime.Parse(parts[1].Split('=')[1]);
            var threadIdInOtp = int.Parse(parts[2].Split('=')[1]);
            if (userIdInOtp != userId)
            {
                logger.LogWarning("User with ID: {UserId} is trying to get a websocket with invalid OTP {HisOTP}.",
                    userId, otp);
                throw new AiurServerException(Code.Unauthorized, "Invalid OTP. User ID does not match.");
            }

            if (threadIdInOtp != threadId)
            {
                logger.LogWarning("User with ID: {UserId} is trying to get a websocket with invalid OTP {HisOTP}.",
                    userId, otp);
                throw new AiurServerException(Code.Unauthorized, "Invalid OTP.");
            }

            if (validTo < DateTime.UtcNow)
            {
                logger.LogWarning("User with ID: {UserId} is trying to get a websocket with expired OTP {HisOTP}.",
                    userId, otp);
                throw new AiurServerException(Code.Unauthorized, "Expired OTP.");
            }
            if (!threadStatuCache.IsUserInThread(userId))
            {
                logger.LogWarning("User with ID: {UserId} is trying to get a websocket for thread {ThreadId} that he is not in.",
                    userId, threadId);
                throw new AiurServerException(Code.Unauthorized, "You are not a member of this thread.");
            }
        }
        catch (CryptographicException)
        {
            logger.LogWarning("User with ID: {UserId} is trying to get a websocket with invalid OTP {HisOTP}.",
                userId, otp);
            throw new AiurServerException(Code.Unauthorized, "Invalid OTP.");
        }
    }
}