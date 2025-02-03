using Aiursoft.ArrayDb.Partitions;
using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.SDK.Services;
using Aiursoft.Kahla.Server.Data;
using Aiursoft.Kahla.Server.Models.Entities;
using Aiursoft.Kahla.Server.Services.Push;

namespace Aiursoft.Kahla.Server.Services.Messages;

/// <summary>
/// 用于向某个 Thread（channel）发送消息，并将结果广播给其他用户、更新缓存、通知推送、落地数据库等等。
/// </summary>
public class ChannelMessageService(
    LocksInMemoryDb locksInMemory,
    ChannelsInMemoryDb channelsInMemoryDb,
    PartitionedObjectBucket<MessageInDatabaseEntity, int> allMessagesDb,
    BufferedKahlaPushService kahlaPushService,
    QuickMessageAccess quickMessageAccess,
    ILogger<ChannelMessageService> logger)
{
    /// <summary>
    /// 将一组新消息发送到某个群聊，对外广播、更新群聊缓存、推送到移动端/在线客户端、落地 DB 等等。
    /// 注意：外部要自行做好并发锁（WriterLock），以免和其它写操作冲突。
    /// </summary>
    /// <param name="newCommits">The new messages to send</param>
    /// <param name="threadId">当前群聊 ID</param>
    /// <param name="senderView">发送者（当前用户）的映射视图，用于推送到前端时序列化</param>
    public async Task SendMessagesToChannel(
        List<Commit<ChatMessage>> newCommits,
        int threadId,
        KahlaUserMappedPublicView senderView)
    {
        logger.LogInformation("User with ID: {UserId} is trying to push a message.", senderView.Id);

        // TODO: Build an additional memory layer to get set if current user has the permission to send messages to this thread.
        // -2. Ensure the user can send messages to this thread
        var threadStatusCache = quickMessageAccess.GetThreadCache(threadId);
        if (!threadStatusCache.IsUserInThread(senderView.Id))
        {
            logger.LogWarning("User with ID: {UserId} is trying to push a message to a thread that he is not in. Rejected.", senderView.Id);
            return;
        }
        
        // -1. Prepare the messages
        var serverTime = DateTime.UtcNow;
        var newMessages = newCommits
            .Select(messageIncoming =>
                MessageInDatabaseEntity.FromPushedCommit(messageIncoming, serverTime, Guid.Parse(senderView.Id), threadId))
            .ToArray();

        if (newMessages.Length == 0)
        {
            logger.LogInformation("No messages to send to thread {ThreadId}.", threadId);
            return;
        }
        
        // 0. Get the lock
        var threadMessagesLock = locksInMemory.GetThreadMessagesLock(threadId);
        threadMessagesLock.EnterWriteLock();
        try
        {
            // 1. Update latest message in cache
            logger.LogTrace("Updating last message for thread {ThreadId}...", threadId);
            threadStatusCache.LastMessage = newMessages[^1].ToSentView(sender: senderView);

            // 2. Bump up the message count
            logger.LogTrace("Bumping up message count for thread {ThreadId}...", threadId);
            threadStatusCache.AppendMessagesCount((uint)newMessages.Length);

            // 3. Broadcast to all users in this thread (ThreadChannel)
            {
                logger.LogTrace("Broadcasting {MessagesCount} messages to thread {ThreadId}...", newMessages.Length,
                    threadId);
                var threadReflector = channelsInMemoryDb.GetThreadChannel(threadId);
                await threadReflector.BroadcastAsync(newMessages);
            }

            // 4. Mark this thread as recently having new messages
            logger.LogTrace("Marking thread {ThreadId} as recently having new messages...", threadId);
            quickMessageAccess.SetThreadAsNewMessageSent(threadId);

            // 5. Handle @s and push to mobile (WebSocket and WebPush channels)
            foreach (var msg in newMessages)
            {
                logger.LogTrace("Handling @s and pushing message {MessageId} to mobile...", msg.Id);
                var atGuids = msg.GetAtsAsGuids();
                foreach (var guid in atGuids)
                {
                    threadStatusCache.AtUser(guid.ToString());
                }

                logger.LogTrace("Pushing message {MessageId} to mobile...", msg.Id);
                kahlaPushService.QueuePushMessageToUsersInThread(
                    threadId,
                    threadStatusCache.ThreadName,
                    msg.ToSentView(senderView),
                    atUserIds: atGuids.Select(g => g.ToString()).ToArray());
            }

            // 6. Save to DB
            allMessagesDb.GetPartitionById(threadId).Add(newMessages);

            // 7. Log
            logger.LogInformation(
                "Successfully sent {MessagesCount} messages to thread {ThreadId}. Last message ID: {LastMessageId}",
                newMessages.Length, threadId, newMessages[^1].Id);
        }
        finally
        {
            threadMessagesLock.ExitWriteLock();
        }
    }
}