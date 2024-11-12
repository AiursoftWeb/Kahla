using System.Collections.Concurrent;
using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.Server.Models;
using Aiursoft.Kahla.Server.Services.Mappers;
using Aiursoft.Kahla.Server.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Data;

/// <summary>
/// The QuickMessageAccess class is responsible for providing an in-memory caching layer to 
/// efficiently track and manage message threads and users' unread message counts. 
/// It combines database initialization data with real-time message tracking to ensure accurate 
/// unread message counts without relying solely on the database for every query.
///
/// ### How Unread Message Count Calculation Works:
/// 1. **Initialization (`BuildAsync`)**:
///    - When the application starts, `BuildAsync` loads the initial state of all message threads 
///      from the database, including the last message in each thread and each user's unread message count.
///    - This unread message count is stored in the `UserUnReadAmountSinceBoot` dictionary, where each user's 
///      unread count reflects the state at the time of the application boot (or restart).
///
/// 2. **Tracking New Messages (`_appendedMessageSinceBootCount`)**:
///    - The `_appendedMessageSinceBootCount` tracks how many new messages have been added to the thread 
///      since the application started.
///    - Each time a new message is added to the thread, the counter is incremented. This allows the system 
///      to calculate how many new messages each user has not seen since the last time they checked.
///
/// 3. **Unread Message Calculation**:
///    - To determine the total number of unread messages for a specific user, the system combines two components:
///      - **Startup Unread Messages**: This is the count of unread messages at the time of application start, 
///        stored in `UserUnReadAmountSinceBoot[UserId]`.
///      - **New Messages Since Boot**: This is the count of messages added to the thread since the application started, 
///        tracked by `_appendedMessageSinceBootCount`.
///    - The sum of these two values gives the total number of unread messages for the user:
///      - `UnreadCount = UserUnReadAmountSinceBoot[UserId] + _appendedMessageSinceBootCount`.
///
/// ### Behavior on Application Restart:
/// - When the application restarts, all caches are rebuilt by reloading the message and thread state from the database.
/// - `UserUnReadAmountSinceBoot` is re-initialized based on the user's current unread messages in the database.
/// - `_appendedMessageSinceBootCount` is reset to zero and starts tracking new messages since the restart.
/// - Since both components are correctly re-initialized after a restart, the unread message count remains accurate 
///   across restarts.
///
/// ### Limitations:
/// - **Cross-Application Restarts**: Since `_appendedMessageSinceBootCount` is reset upon restart, any dynamic 
///   message count tracking is lost between restarts. However, the system recalculates unread counts from the 
///   database, ensuring no data is lost.
/// - **Distributed Systems**: In distributed scenarios where multiple application instances are running, the cache 
///   is stored in-memory on each node, meaning that each instance must ensure cache consistency across nodes. 
///   This design is currently optimal for single-instance environments or environments with synchronized caches.
///
/// Overall, the QuickMessageAccess class ensures an efficient and accurate method for tracking unread messages, 
/// leveraging a hybrid of in-memory cache and database-backed initialization to provide real-time message updates 
/// and consistency even in the case of application restarts.
/// </summary>
public class QuickMessageAccess(
    ArrayDbContext arrayDbContext,
    IServiceScopeFactory scopeFactory,
    ILogger<QuickMessageAccess> logger)
{
    private ConcurrentDictionary<int, ThreadsInMemoryCache> CachedThreads { get; } = new();

    public async Task LoadAsync()
    {
        var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<KahlaRelationalDbContext>();
        var userOthersViewRepo = scope.ServiceProvider.GetRequiredService<UserOthersViewRepo>();
        logger.LogInformation("Building quick message access cache.");
        foreach (var thread in dbContext.ChatThreads)
        {
            logger.LogInformation("Building cache for thread with ID {ThreadId}.", thread.Id);
            var lastMessageEntity = arrayDbContext.GetLastMessage(thread.Id);
            var lastMessage = lastMessageEntity?.Map(await userOthersViewRepo.GetUserByIdWithCacheAsync(lastMessageEntity.SenderId.ToString("D")));
            
            var membersInThread = await dbContext
                .UserThreadRelations
                .AsNoTracking()
                .Where(t => t.ThreadId == thread.Id)
                .ToListAsync();
            var userUnReadAmountSinceBoot = new ConcurrentDictionary<string, int>();
            var totalMessages = arrayDbContext.GetTotalMessagesCount(thread.Id);
            foreach (var member in membersInThread)
            {
                var unReadMessages = totalMessages - member.ReadMessageIndex;
                logger.LogInformation("Cache built for user with ID {UserId} in thread with ID {ThreadId}. His un-read message count is {UnReadMessages}.", member.UserId, thread.Id, unReadMessages);
                userUnReadAmountSinceBoot.TryAdd(member.UserId, unReadMessages);
            }
            
            var threadInMemoryCache = new ThreadsInMemoryCache
            {
                LastMessage = lastMessage,
                UserUnReadAmountSinceBoot = userUnReadAmountSinceBoot
            };
            CachedThreads.TryAdd(thread.Id, threadInMemoryCache);
            logger.LogInformation("Cache built for thread with ID {ThreadId}. Last message time: {LastMessageTime}.", thread.Id, lastMessage?.SendTime);
        }
        logger.LogInformation("Quick message access cache built. Totally {ThreadCount} threads cached.", CachedThreads.Count);
    }

    /// <summary>
    /// This method will update the in-memory cache of a thread.
    ///
    /// To call this message, please make sure the message is already saved in the database. And the Sender of the message is already loaded.
    /// </summary>
    /// <param name="lastMessage"></param>
    /// <param name="messagesCount"></param>
    public void OnNewMessagesSent(KahlaMessageMappedSentView lastMessage, uint messagesCount)
    {
        var threadCache = CachedThreads[lastMessage.ThreadId];
        lock (threadCache)
        {
            // Set as new last message.
            threadCache.LastMessage = lastMessage;
        }
        
        // Increase the appended message count. So all users will see this message as unread.
        threadCache.AppendMessage(messagesCount);
    }
    
    /// <summary>
    /// This should be called if the user has read all messages in this thread.
    /// </summary>
    /// <param name="threadId"></param>
    /// <param name="userId"></param>
    public void ClearUserUnReadAmountForUser(int threadId, string userId)
    {
        var threadCache = CachedThreads[threadId];
        lock (threadCache)
        {
            threadCache.ClearUserUnReadAmountSinceBoot(userId);
        }
    }
    
    /// <summary>
    /// This should be called when a new thread is created.
    /// </summary>
    /// <param name="threadId"></param>
    public void OnNewThreadCreated(int threadId)
    {
        CachedThreads.TryAdd(threadId, new ThreadsInMemoryCache
        {
            LastMessage = null,
            UserUnReadAmountSinceBoot = new ConcurrentDictionary<string, int>()
        });
    }
    
    /// <summary>
    /// This should be called when a thread is deleted.
    /// </summary>
    /// <param name="threadId"></param>
    public void OnThreadDropped(int threadId)
    {
        CachedThreads.TryRemove(threadId, out _);
    }

    public MessageContext GetMessageContext(int tId, string viewingUserId)
    {
        var chatThread = CachedThreads[tId];
        return new MessageContext
        {
            UnReadAmount = chatThread.GetUserUnReadAmount(viewingUserId),
            LatestMessage = chatThread.LastMessage
        };
    }
}