using Aiursoft.ArrayDb.Partitions;
using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.Server.Data;
using Aiursoft.Kahla.Server.Models.Entities;
using Aiursoft.Kahla.Server.Services.Push;

namespace Aiursoft.Kahla.Server.Services.Messages;

/// <summary>
/// 用于向某个 Thread（channel）发送消息，并将结果广播给其他用户、更新缓存、通知推送、落地数据库等等。
/// </summary>
public class ChannelMessageService(
    ChannelsInMemoryDb channelsInMemoryDb,
    PartitionedObjectBucket<MessageInDatabaseEntity, int> allMessagesDb,
    BufferedKahlaPushService kahlaPushService,
    QuickMessageAccess quickMessageAccess)
{
    /// <summary>
    /// 将一组新消息发送到某个群聊，对外广播、更新群聊缓存、推送到移动端/在线客户端、落地 DB 等等。
    /// 注意：外部要自行做好并发锁（WriterLock），以免和其它写操作冲突。
    /// </summary>
    /// <param name="newMessages">需要发送的消息集合（已经构造好的实体）</param>
    /// <param name="threadId">当前群聊 ID</param>
    /// <param name="senderView">发送者（当前用户）的映射视图，用于推送到前端时序列化</param>
    /// <param name="logger">Logger</param>
    public async Task SendMessagesInChannel(
        MessageInDatabaseEntity[] newMessages,
        int threadId,
        KahlaUserMappedPublicView senderView,
        ILogger logger)
    {
        // TODO: Build an additional memory layer to get set if current user has the permission to send messages to this thread.

        if (newMessages.Length == 0)
        {
            logger.LogInformation("No messages to send to thread {ThreadId}.", threadId);
            return;
        }
        
        var threadStatusCache = quickMessageAccess.GetThreadCache(threadId);

        // 1. Update last message
        threadStatusCache.LastMessage = newMessages[^1].ToSentView(sender: senderView);

        // 2. Bump up the message count
        threadStatusCache.AppendMessagesCount((uint)newMessages.Length);

        // 3. Broadcast to all users in this thread
        {
            var threadReflector = channelsInMemoryDb.GetThreadChannel(threadId);
            await threadReflector.BroadcastAsync(newMessages);
        }

        // 4. Mark this thread as recently having new messages
        quickMessageAccess.SetThreadAsNewMessageSent(threadId);

        // 5. Handle @s and push to mobile
        foreach (var msg in newMessages)
        {
            var atGuids = msg.GetAtsAsGuids();
            foreach (var guid in atGuids)
            {
                threadStatusCache.AtUser(guid.ToString());
            }

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
}
