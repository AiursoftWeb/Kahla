using System.Security.Cryptography;
using System.Text;
using Aiursoft.AiurObserver;
using Aiursoft.AiurObserver.WebSocket.Server;
using Aiursoft.AiurProtocol.Models;
using Aiursoft.AiurProtocol.Server.Attributes;
using Aiursoft.Kahla.Server.Attributes;
using Aiursoft.Kahla.Server.Data;
using Microsoft.AspNetCore.Mvc;
using Aiursoft.AiurObserver.WebSocket;
using Aiursoft.AiurProtocol.Exceptions;
using Aiursoft.AiurProtocol.Server;
using Aiursoft.ArrayDb.ObjectBucket;
using Aiursoft.ArrayDb.ObjectBucket.Abstractions.Interfaces;
using Aiursoft.ArrayDb.Partitions;
using Aiursoft.Kahla.SDK.Events;
using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.SDK.Models.ViewModels;
using Aiursoft.Kahla.SDK.Services;
using Aiursoft.Kahla.Server.Models;
using Aiursoft.Kahla.Server.Models.Entities;
using Aiursoft.Kahla.Server.Services;
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
    BufferedKahlaPushService kahlaPushService,
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
    public async Task<IActionResult> WebSocket([FromRoute] string userId, [FromQuery] string otp)
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
        var channel = memoryDb.GetMyChannel(userId);
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
        var threadReflector = memoryDb.GetThreadNewMessagesChannel(threadId);
        var threadMessagesLock = locksInMemory.GetThreadMessagesLock(threadId);
        var user = await relationalDbContext.Users.FindAsync(userId);

        logger.LogInformation("User with ID: {UserId} is trying to connect to thread {ThreadId}.", userId, threadId);
        var socket = await HttpContext.AcceptWebSocketClient();
        logger.LogInformation("User with ID: {UserId} is accepted to connect to thread {ThreadId}.", userId, threadId);
        
        var clientPushConsumer = new ClientPushConsumer(
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
            kahlaPushService,
            threadId,
            quickMessageAccess,
            threadCache,
            logger,
            threadMessagesLock,
            Guid.Parse(userId),
            threadReflector,
            messagesDb);
        var reflectorConsumer = new ThreadReflectConsumer(
            quickMessageAccess,
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
            var readLength = GetReadLength(messagesDb, startLocation);
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
        
        // Clear current user's unread message count.
        quickMessageAccess.ClearUserUnReadAmountForUser(threadCache, userId);
        
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

    private void EnsureUserIsMemberOfThread(int threadId, string userId, string otp, ThreadsInMemoryCache threadCache)
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
            if (!threadCache.IsUserInThread(userId))
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

    private static int GetReadLength(
        IObjectBucket<MessageInDatabaseEntity> messagesDb,
        int startLocation)
    {
        var readLength = messagesDb.Count - startLocation;
        return readLength;
    }
}

public class ClientPushConsumer(
    KahlaUserMappedPublicView userView,
    BufferedKahlaPushService kahlaPushService,
    int threadId,
    QuickMessageAccess quickMessageAccess,
    ThreadsInMemoryCache threadCache,
    ILogger<MessagesController> logger,
    ReaderWriterLockSlim threadMessagesLock,
    Guid userIdGuid,
    AsyncObservable<MessageInDatabaseEntity[]> threadReflector,
    IObjectBucket<MessageInDatabaseEntity> messagesDb)
    : IConsumer<string>
{
    public async Task Consume(string clientPushed)
    {
        if (clientPushed.Length > 0xFFFF)
        {
            logger.LogWarning("User with ID: {UserId} is trying to push a message that is too large. Rejected. Max allowed size is 65535. He pushed {Size} bytes.", userIdGuid, clientPushed.Length);
            return;
        }
        if (!threadCache.IsUserInThread(userIdGuid.ToString()))
        {
            logger.LogWarning("User with ID: {UserId} is trying to push a message to a thread that he is not in. Rejected.", userIdGuid);
            return;
        }
        
        logger.LogInformation("User with ID: {UserId} is trying to push a message.", userIdGuid);
        threadMessagesLock.EnterWriteLock();
        try
        {
            // TODO: The thread may be muted that not allowing anyone to send new messages. In this case, don't allow him to do this.
            // Deserialize the incoming messages and fill the properties.
            var model = SDK.Extensions.Deserialize<List<Commit<ChatMessage>>>(clientPushed);
            var serverTime = DateTime.UtcNow;
            var messagesToAddToDb = model
                .Select(messageIncoming => MessageInDatabaseEntity.FromPushedCommit(messageIncoming, serverTime, userIdGuid))
                .ToArray();

            // TODO: Build an additional memory layer to get set if current user has the permission to send messages to this thread.
            
            // Reflect in quick message access layer.
            if (messagesToAddToDb.Any())
            {
                // Set as new last message in cache.
                {
                    var lastMessage = messagesToAddToDb.Last();
                    threadCache.LastMessage = new KahlaMessageMappedSentView
                    {
                        Id = lastMessage.Id,
                        ThreadId = threadId,
                        Preview = Encoding.UTF8.GetString(lastMessage.Preview.TrimEndZeros()),
                        SendTime = lastMessage.CreationTime,
                        Sender =
                            userView // This userView is cached during the user is connected. If he changes his profile, this will not be updated.
                    };
                }

                // Increase the appended message count. So all users will see this message as unread.
                threadCache.AppendMessagesCount((uint)messagesToAddToDb.Length);
                
                // Reflect to other clients.
                await threadReflector.BroadcastAsync(messagesToAddToDb);
                
                // Set the thread as new message sent.
                quickMessageAccess.SetThreadAsNewMessageSent(threadId);
            }

            // Save to database.
            messagesDb.Add(messagesToAddToDb);
            
            // Push to other users.
            kahlaPushService.QueuePushEventsToUsersInThread(threadId: threadId, PushMode.AllPath, new NewMessageEvent
            {
                Message = new KahlaMessageMappedSentView
                {
                    Id = messagesToAddToDb.Last().Id,
                    Preview = Encoding.UTF8.GetString(messagesToAddToDb.Last().Preview.TrimEndZeros()),
                    Sender = userView,
                    SendTime = messagesToAddToDb.Last().CreationTime,
                    ThreadId = threadId
                }
            });
            
            logger.LogInformation(
                "User with ID: {UserId} pushed {Count} messages. We have successfully broadcast to other clients and saved to database.",
                userIdGuid, messagesToAddToDb.Length);
        }
        finally
        {
            threadMessagesLock.ExitWriteLock();
        }
    }
}

public class ThreadReflectConsumer(
    //UserOthersViewRepo usersRepo,
    //BufferedKahlaPushService kahlaPushService,
    QuickMessageAccess quickMessageAccess,
    ThreadsInMemoryCache threadCache,
    string listeningUserId,
    ILogger<MessagesController> logger,
    ObservableWebSocket socket)
    : IConsumer<MessageInDatabaseEntity[]>
{
    public async Task Consume(MessageInDatabaseEntity[] newEntities)
    {
        // Ensure the user is in the thread.
        if (!threadCache.IsUserInThread(listeningUserId))
        {
            logger.LogWarning("User with ID: {UserId} is trying to listen to a thread that he is not in. Rejected.", listeningUserId);
            return;
        }
        
        // Send to the client.
        logger.LogInformation("Reflecting {Count} new messages to the client with Id: '{ClientId}'.", newEntities.Length, listeningUserId);
        await socket.Send(SDK.Extensions.Serialize(newEntities.Select(t => t.ToCommit())));

        // var messageEvents = new List<NewMessageEvent>();
        // foreach (var entity in newEntities)
        // {
        //     var sender = await usersRepo.GetUserByIdWithCacheAsync(entity.SenderId.ToString());
        //     if (sender == null)
        //     {
        //         logger.LogWarning("User with ID: {UserId} is trying to push a message to a thread that he is not in. Rejected.", listeningUserId);
        //         continue;
        //     }
        //     var messageEvent = new NewMessageEvent
        //     {
        //         Message = new KahlaMessageMappedSentView
        //         {
        //             Id = entity.Id,
        //             ThreadId = threadCache.ThreadId,
        //             Sender = new KahlaUserMappedPublicView
        //             {
        //                 Id = sender.Id,
        //                 NickName = sender.NickName,
        //                 Bio = sender.Bio,
        //                 IconFilePath = sender.IconFilePath,
        //                 AccountCreateTime = sender.AccountCreateTime,
        //                 EmailConfirmed = sender.EmailConfirmed,
        //                 Email = sender.Email
        //             },
        //             Preview = Encoding.UTF8.GetString(entity.Preview.TrimEndZeros()),
        //             SendTime = entity.CreationTime,
        //         },
        //         Muted = false,
        //     };
        //     messageEvents.Add(messageEvent);
        // }
        // kahlaPushService.QueuePushToUser(listeningUserId, PushMode.AllPath, messageEvents);

        // Clear current user's unread message count.
        quickMessageAccess.ClearUserUnReadAmountForUser(threadCache, listeningUserId);
    }
}