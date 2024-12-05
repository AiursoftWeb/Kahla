using Aiursoft.Canon;
using Aiursoft.Kahla.SDK.Events;
using Aiursoft.Kahla.Server.Data;

namespace Aiursoft.Kahla.Server.Services.Push;

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
            var shouldPush = (!muted || atTargeted) && !userIsSender;
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