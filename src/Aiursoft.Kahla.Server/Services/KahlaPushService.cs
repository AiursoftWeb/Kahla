using Aiursoft.Canon;
using Aiursoft.Kahla.SDK.Events;
using Aiursoft.Kahla.Server.Data;

namespace Aiursoft.Kahla.Server.Services;

public enum PushMode
{
    AllPath,
    OnlyWebPush,
    OnlyWebSocket,
    DryRun
}

public class BufferedKahlaPushService(
    ILogger<BufferedKahlaPushService> logger,
    QuickMessageAccess quickMessageAccess,
    CanonQueue canonQueue)
{
    private void QueuePushEventsToUser(string userId, PushMode mode, IEnumerable<KahlaEvent> payloads)
    {
        canonQueue.QueueWithDependency<KahlaPushService>(async p =>
        {
            foreach (var payload in payloads)
            {
                await p.PushToUser(userId, payload, mode);
            }
        });
    }

    /// <summary>
    /// TODO: Add new switch: Only for users (!thread-muted || at-targeted) && !isSendingUser
    /// </summary>
    /// <param name="threadId"></param>
    /// <param name="mode"></param>
    /// <param name="payload"></param>
    public void QueuePushEventsToUsersInThread(int threadId, PushMode mode, KahlaEvent payload)
    {
        logger.LogInformation("Pushing payload with type {Type} to all users in thread: {ThreadId} via mode: {Mode}", payload.GetType().Name, threadId, mode);
        var usersInThread = quickMessageAccess.GetUsersInThread(threadId);
        foreach (var cachedUserInThreadInfo in usersInThread)
        {
            QueuePushEventsToUser(cachedUserInThreadInfo.UserId, mode, [payload]);
        }
    }
    
    public void QueuePushMessageToUsersInThread(int threadId, NewMessageEvent payload, string[]? atUserIds = null)
    {
        var usersInThread = quickMessageAccess.GetUsersInThread(threadId);
        foreach (var cachedUserInThreadInfo in usersInThread)
        {
            var muted = cachedUserInThreadInfo.Muted;
            var atTargeted = atUserIds?.Contains(cachedUserInThreadInfo.UserId) ?? false;
            var userIsSender = cachedUserInThreadInfo.UserId == payload.Message.Sender?.Id;
            var shouldPush = !muted && (atTargeted || userIsSender);
            var reason = 
                (!muted ? "User didn't mute the thread. " : "User muted this thread. ") + 
                (atTargeted ? "The user is at-targeted. " : " the user is not at-targeted. ") +
                (userIsSender ? "The user is the sender." : "The user is not the sender.");
            if (shouldPush)
            {
                logger.LogInformation("Pushing web push message to user: {UserId} in thread {ThreadId} because {Reason}",
                    cachedUserInThreadInfo.UserId, threadId, reason);
                QueuePushEventsToUser(cachedUserInThreadInfo.UserId, PushMode.AllPath, [payload]);
            }
            else
            {
                logger.LogInformation("Don't have to push to user: {UserId} in thread {ThreadId} because {Reason} But we will still push to websocket.",
                    cachedUserInThreadInfo.UserId, threadId, reason);
                QueuePushEventsToUser(cachedUserInThreadInfo.UserId, PushMode.OnlyWebSocket, [payload]);
            }
        }
    }

    public void QueuePushEventToUser(string userId, PushMode mode, KahlaEvent payload)
    {
        logger.LogInformation("Pushing payload with type {Type} to user: {UserId} via mode: {Mode}", payload.GetType().Name, userId, mode);
        QueuePushEventsToUser(userId, mode, [payload]);
    }

    public async Task Sync()
    {
        await canonQueue.Engine;
    }
}

public class KahlaPushService(
    DevicesCache devicesCache,
    ILogger<KahlaPushService> logger,
    KahlaRelationalDbContext context,
    CanonPool canonPool,
    WebSocketPushService wsPusher,
    WebPushService webPusher)
{
    public async Task PushToUser(string userId, KahlaEvent payload, PushMode mode = PushMode.AllPath)
    {
        // WebSocket push.
        if (mode is PushMode.AllPath or PushMode.OnlyWebSocket)
        {
            logger.LogInformation("Pushing to user: {UserId} with WebSocket...", userId);
            canonPool.RegisterNewTaskToPool(async () => { await wsPusher.PushAsync(userId, payload); });
        }

        // Web push.
        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (mode is PushMode.AllPath or PushMode.OnlyWebPush)
        {
            // Load his devices
            var hisDevices = await devicesCache.GetValidDevicesWithCache(userId);
            logger.LogInformation("Pushing to user: {UserId} with {DeviceCount} WebPush devices...", userId,
                hisDevices.Count);
            foreach (var hisDevice in hisDevices)
            {
                canonPool.RegisterNewTaskToPool(async () => { await webPusher.PushAsync(hisDevice, payload); });
            }
        }

        // Dry run.
        if (mode is PushMode.DryRun)
        {
            logger.LogWarning("Dry run mode is enabled. No push will be sent.");
        }

        // Do actual push.
        await canonPool.RunAllTasksInPoolAsync(Extensions.GetLimitedNumber(
            min: 8,
            max: 32,
            suggested: Environment.ProcessorCount));
        
        // Some devices may be invalid, remove them.
        await context.SaveChangesAsync();
    }
}