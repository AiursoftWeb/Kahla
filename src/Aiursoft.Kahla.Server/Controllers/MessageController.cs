using System.Security.Cryptography;
using Aiursoft.AiurEventSyncer.Abstract;
using Aiursoft.AiurEventSyncer.ConnectionProviders.Models;
using Aiursoft.AiurObserver;
using Aiursoft.AiurObserver.WebSocket.Server;
using Aiursoft.AiurProtocol.Models;
using Aiursoft.AiurProtocol.Server.Attributes;
using Aiursoft.Kahla.Server.Attributes;
using Aiursoft.Kahla.Server.Data;
using Microsoft.AspNetCore.Mvc;
using Aiursoft.AiurObserver.Extensions;
using Aiursoft.AiurObserver.WebSocket;
using Aiursoft.AiurProtocol.Exceptions;
using Aiursoft.AiurProtocol.Server;
using Aiursoft.ArrayDb.ObjectBucket;
using Aiursoft.ArrayDb.Partitions;
using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.SDK.Models.ViewModels;
using Aiursoft.Kahla.Server.Models.Entities;
using Aiursoft.WebTools.Attributes;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Controllers;

[LimitPerMin]
[ApiExceptionHandler(
    PassthroughRemoteErrors = true,
    PassthroughAiurServerException = true)]
[ApiModelStateChecker]
[Route("api/messages")]
public class MessageController(
    QuickMessageAccess quickMessageAccess,
    LocksInMemoryDb locksInMemory,
    PartitionedObjectBucket<MessageInDatabaseEntity, int> messages,
    IDataProtectionProvider dataProtectionProvider,
    ChannelsInMemoryDb memoryDb,
    KahlaRelationalDbContext relationalDbContext,
    ILogger<MessageController> logger) : ControllerBase
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
        logger.LogInformation("User with Id: {Id} is trying to init a channel websocket OTP.", currentUserId);
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
        var outSub = channel.Subscribe(t => pusher.Send(t, HttpContext.RequestAborted));

        try
        {
            await pusher.Listen(HttpContext.RequestAborted);
        }
        catch (TaskCanceledException)
        {
            // Ignore. This happens when the client closes the connection.
        }
        catch (ConnectionAbortedException)
        {
            // Ignore. This happens when the client closes the connection.
        }
        finally
        {
            logger.LogInformation("User with Id: {Id} closed the websocket.", userId);
            await pusher.Close(HttpContext.RequestAborted);
            outSub.Unsubscribe();
        }

        return new EmptyResult();
    }

    [EnforceWebSocket]
    [Route("websocket-thread/{threadId:int}/{userId}/{otp}")]
    public async Task<IActionResult> WebSocketForThread(
        [FromRoute] int threadId,
        [FromRoute] string userId,
        [FromRoute] string otp,
        [FromQuery] string? start)
    {
        await EnsureUserJoined(threadId, userId, otp);

        var messagesDb = messages.GetPartitionById(threadId);
        var threadReflector = memoryDb.GetThreadNewMessagesChannel(threadId);
        var lockObject = locksInMemory.GetThreadMessagesLock(threadId);
        var user = await relationalDbContext.Users.FindAsync(userId);

        logger.LogInformation("User with ID: {UserId} is trying to connect to thread {ThreadId}.", userId, threadId);
        var socket = await HttpContext.AcceptWebSocketClient();

        ISubscription? reflectorSubscription;
        ISubscription? clientSubscription;

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
            threadId,
            quickMessageAccess,
            logger,
            lockObject,
            Guid.Parse(userId),
            threadReflector,
            messagesDb);
        var reflectorConsumer = new ThreadReflectConsumer(
            userId,
            logger,
            socket);

        lockObject.EnterReadLock();
        try
        {
            var (startLocation, readLength) = GetInitialReadLocation(messagesDb, start);
            if (readLength > 0)
            {
                logger.LogInformation(
                    "User with ID: {UserId} is trying to pull {ReadLength} messages from thread {ThreadId}. Start Index is {StartIndex}",
                    userId, readLength, threadId, startLocation);
                var firstPullRequest = messagesDb.ReadBulk(startLocation, readLength);
                await socket.Send(Extensions.Serialize(firstPullRequest.Select(t => new Commit<ChatMessage>
                {
                    Item = t.ToClientView(),
                    Id = t.Id.ToString("D"),
                    CommitTime = t.CreationTime
                })));
            }

            reflectorSubscription = threadReflector.Subscribe(reflectorConsumer);
            clientSubscription = socket.Subscribe(clientPushConsumer);
        }
        finally
        {
            lockObject.ExitReadLock();
        }

        try
        {
            logger.LogInformation("User with ID: {UserId} connected to thread {ThreadId} and listening for new events.",
                userId, threadId);
            await socket.Listen(HttpContext.RequestAborted);
        }
        catch (TaskCanceledException)
        {
            // Ignore. This happens when the client closes the connection.
        }
        catch (ConnectionAbortedException)
        {
            // Ignore. This happens when the client closes the connection.
        }
        finally
        {
            reflectorSubscription.Unsubscribe();
            clientSubscription.Unsubscribe();
            if (socket.Connected)
            {
                await socket.Close(HttpContext.RequestAborted);
            }
        }

        return new EmptyResult();
    }

    private async Task EnsureUserJoined(int threadId, string userId, string otp)
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

            var thread = await relationalDbContext.ChatThreads.FindAsync(threadIdInOtp);
            if (thread == null)
            {
                throw new AiurServerException(Code.NotFound, "The thread does not exist.");
            }

            var myRelation = await relationalDbContext.UserThreadRelations
                .Where(t => t.UserId == userId)
                .Where(t => t.ThreadId == threadIdInOtp)
                .FirstOrDefaultAsync();
            if (myRelation == null)
            {
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

    private static (int startOffset, int readLength) GetInitialReadLocation(
        IObjectBucket<MessageInDatabaseEntity> messagesDb,
        string? start)
    {
        var startLocation = 0;
        var found = false;

        if (!string.IsNullOrWhiteSpace(start))
        {
            var startGuid = Guid.Parse(start);
            startLocation = messagesDb.Count;

            // TODO: Really really bad performance. O(n) search.
            // Refactor required. Replace this with a hash table with LRU.
            foreach (var message in messagesDb.AsReverseEnumerable())
            {
                if (message.Id == startGuid)
                {
                    found = true;
                    break;
                }

                startLocation--;
            }
        }

        if (!found)
        {
            startLocation = 0;
        }

        var readLength = messagesDb.Count - startLocation;
        return (startLocation, readLength);
    }
}

public class ClientPushConsumer(
    KahlaUserMappedPublicView userView,
    int threadId,
    QuickMessageAccess quickMessageAccess,
    ILogger<MessageController> logger,
    ReaderWriterLockSlim lockObject,
    Guid userIdGuid,
    AsyncObservable<MessageInDatabaseEntity[]> threadReflector,
    IObjectBucket<MessageInDatabaseEntity> messagesDb)
    : IConsumer<string>
{
    public async Task Consume(string clientPushed)
    {
        logger.LogInformation("User with ID: {UserId} is trying to push a message.", userIdGuid);
        lockObject.EnterWriteLock();
        try
        {
            // TODO: The thread may be muted that not allowing anyone to send new messages. In this case, don't allow him to do this.
            // Deserialize the incoming messages and fill the properties.
            var model = Extensions.Deserialize<PushModel<ChatMessage>>(clientPushed);
            var serverTime = DateTime.UtcNow;
            var messagesToAddToDb = model.Commits
                .Select(messageIncoming => new MessageInDatabaseEntity
                {
                    Content = messageIncoming.Item.Content,
                    Id = Guid.Parse(messageIncoming.Id),
                    CreationTime = serverTime,
                    SenderId = userIdGuid,
                })
                .ToArray();

            // TODO: Build an additional memory layer to get set if current user has the permission to send messages to this thread.
            // Reflect to other clients.
            await threadReflector.BroadcastAsync(messagesToAddToDb);

            // Reflect in quick message access layer.
            var lastMessage = messagesToAddToDb.Last();
            quickMessageAccess.OnNewMessagesSent(
                lastMessage: new KahlaMessageMappedSentView
                {
                    Id = lastMessage.Id,
                    ThreadId = threadId,
                    Content = lastMessage.Content,
                    SendTime = lastMessage.CreationTime,
                    Sender = userView
                },
                messagesCount: (uint)messagesToAddToDb.Length);

            // Save to database.
            messagesDb.Add(messagesToAddToDb);
            logger.LogInformation(
                "User with ID: {UserId} pushed {Count} messages. We have successfully broadcast to other clients and saved to database.",
                userIdGuid, messagesToAddToDb.Length);
        }
        finally
        {
            lockObject.ExitWriteLock();
        }
    }
}

public class ThreadReflectConsumer(
    string listeningUserId,
    ILogger<MessageController> logger,
    ObservableWebSocket socket)
    : IConsumer<MessageInDatabaseEntity[]>
{
    public async Task Consume(MessageInDatabaseEntity[] newCommits)
    {
        logger.LogInformation("Reflecting {Count} new messages to the client with Id: '{ClientId}'.", newCommits.Length, listeningUserId);
        await socket.Send(Extensions.Serialize(newCommits.Select(t => new Commit<ChatMessage>
        {
            Item = t.ToClientView(),
            Id = t.Id.ToString("D"),
            CommitTime = t.CreationTime
        })));
    }
}