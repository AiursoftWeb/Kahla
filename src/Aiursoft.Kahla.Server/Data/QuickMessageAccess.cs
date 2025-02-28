using System.Collections.Concurrent;
using Aiursoft.Kahla.Entities;
using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.Server.Models;
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
    private ConcurrentDictionary<int, ThreadStatusInMemoryCache> CachedThreads { get; } = new();

    /// <summary>
    /// This list is actually sorted by the thread's last message time.
    ///
    /// The newer, the front.
    ///
    /// If there is no message in the thread, it will be sorted with its creation time.
    /// </summary>
    private LinkedList<int> ThreadIdsSortedByLastMessageTime { get; } = new();

    private ReaderWriterLockSlim ThreadIdsSortedByLastMessageTimeLock { get; } = new();

    public async Task LoadAsync()
    {
        // Clean up.
        if (CachedThreads.Any())
        {
            CachedThreads.Clear();
        }

        if (ThreadIdsSortedByLastMessageTime.Any())
        {
            ThreadIdsSortedByLastMessageTime.Clear();
        }

        var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<KahlaRelationalDbContext>();
        var userOthersViewRepo = scope.ServiceProvider.GetRequiredService<UserOthersViewRepo>();
        logger.LogInformation("Building quick message access cache...(This happens when the application starts, only once.)");
        var threads = await dbContext
            .ChatThreads
            .Include(t => t.Members)
            .AsNoTracking()
            .ToListAsync();
        foreach (var thread in threads)
        {
            // TODO: Function is too ugly. Refactor it to multiple functions.
            logger.LogInformation("Building cache for thread with ID {ThreadId}...", thread.Id);
            
            // Build lastMessage.
            var lastMessageEntity = arrayDbContext.GetLastMessage(thread.Id);
            KahlaMessageMappedSentView? lastMessage = null;
            if (lastMessageEntity != null)
            {
                var user = await userOthersViewRepo.GetUserByIdWithCacheAsync(lastMessageEntity.SenderId.ToString("D"));
                var userMapped = user == null ? null : new KahlaUserMappedPublicView
                {
                    Id = user.Id,
                    NickName = user.NickName,
                    Bio = user.Bio,
                    IconFilePath = user.IconFilePath,
                    AccountCreateTime = user.AccountCreateTime,
                    EmailConfirmed = user.EmailConfirmed,
                    Email = user.Email
                };
                lastMessage = lastMessageEntity.ToSentView(userMapped);
            }
            
            // Build userInfos.
            var membersInThread = thread.Members;
            var userInfos = new ConcurrentDictionary<string, CachedUserInThreadInfo>();
            var totalMessages = arrayDbContext.GetTotalMessagesCount(thread.Id);
            foreach (var member in membersInThread)
            {
                // Build unReadMessages.
                var unReadMessagesCount = totalMessages - member.ReadMessageIndex;
                
                // Build beingAted.
                var unreadRange = arrayDbContext.ReadBulk(
                    thread.Id, 
                    start: member.ReadMessageIndex, 
                    count: unReadMessagesCount);
                var beingAted = unreadRange.Any(r => 
                    r.GetAtsAsGuids().Any(g => g.ToString() == member.UserId)); 
                
                userInfos.TryAdd(member.UserId, new CachedUserInThreadInfo
                {
                    UserId = member.UserId,
                    UnreadAmountSinceBoot = unReadMessagesCount,
                    Muted = member.Muted,
                    BeingAted = beingAted
                });
                
                logger.LogInformation(
                    "Cache built for user with ID {UserId} in thread with ID {ThreadId}. His un-read message count is {UnReadMessages}. He is being ated: {BeingAted}.",
                    member.UserId, thread.Id, unReadMessagesCount, beingAted);
            }

            var threadInMemoryCache = new ThreadStatusInMemoryCache
            {
                ThreadId = thread.Id,
                LastMessage = lastMessage,
                UserInfo = userInfos,
                ThreadCreatedTime = thread.CreateTime,
                ThreadName = thread.Name
            };
            CachedThreads[thread.Id] = threadInMemoryCache;
            logger.LogInformation("Cache built for thread with ID {ThreadId}. Last message time: {LastMessageTime}.",
                thread.Id, lastMessage?.SendTime);

            // Insert the thread into the sorted linked list using the new method.
            InsertThreadIntoSortedList(thread.Id, lastMessage?.SendTime ?? thread.CreateTime);

            logger.LogInformation("Thread with ID {ThreadId} inserted into the sorted list.", thread.Id);
        }
        
        if (CachedThreads.Count != ThreadIdsSortedByLastMessageTime.Count)
        {
            throw new InvalidOperationException(
                $"The count of cached threads and the count of threads in the sorted list are not equal! Cached threads: {CachedThreads.Count}, Sorted list count: {ThreadIdsSortedByLastMessageTime.Count}. Is the data corrupted?");
        }

        logger.LogInformation(
            "Quick message access cache built and is ready to be used. Totally {ThreadCount} threads cached. {ListCount} items in sorted linked list.",
            CachedThreads.Count, ThreadIdsSortedByLastMessageTime.Count);
    }
    
    /// <summary>
    /// Inserts a thread ID into the sorted linked list based on the provided last message time.
    /// Newer threads are placed at the front of the list.
    /// </summary>
    /// <param name="threadId">The ID of the thread to insert.</param>
    /// <param name="lastMessageTime">The timestamp of the last message in the thread.</param>
    private void InsertThreadIntoSortedList(int threadId, DateTime lastMessageTime)
    {
        ThreadIdsSortedByLastMessageTimeLock.EnterWriteLock();
        try
        {
            var node = ThreadIdsSortedByLastMessageTime.First;
            while (node != null)
            {
                var currentThread = CachedThreads[node.Value];
                var currentLastMessageTime = currentThread.LastMessage?.SendTime ?? currentThread.ThreadCreatedTime;

                if (lastMessageTime > currentLastMessageTime)
                {
                    ThreadIdsSortedByLastMessageTime.AddBefore(node, threadId);
                    return;
                }

                node = node.Next;
            }

            // If no node has a lesser timestamp, add to the end.
            ThreadIdsSortedByLastMessageTime.AddLast(threadId);
        }
        finally
        {
            ThreadIdsSortedByLastMessageTimeLock.ExitWriteLock();
        }
    }

    /// <summary>
    /// This method will update the in-memory cache of a thread to move it to the front of the sorted list.
    ///
    /// This indicates that this thread has new messages sent.
    /// </summary>
    /// <param name="threadId">The thread ID.</param>
    public void SetThreadAsNewMessageSent(int threadId)
    {
        // Move the thread to the last of the linked list.
        ThreadIdsSortedByLastMessageTimeLock.EnterWriteLock();
        try
        {
            // If already on the first, no need to move.
            if (ThreadIdsSortedByLastMessageTime.First?.Value == threadId)
            {
                return;
            }
            
            // Move the id to the front.
            if (ThreadIdsSortedByLastMessageTime.Remove(threadId))
            {
                // Only move to the front if it was in the list.
                ThreadIdsSortedByLastMessageTime.AddFirst(threadId);
            }
        }
        finally
        {
            ThreadIdsSortedByLastMessageTimeLock.ExitWriteLock();
        }
    }

    public void OnThreadNameChanged(int threadId, string newName)
    {
        CachedThreads[threadId].ThreadName = newName;
    }

    /// <summary>
    /// This should be called when a new thread is created.
    /// </summary>
    /// <param name="threadId"></param>
    /// <param name="createTime"></param>
    /// <param name="threadName"></param>
    public void OnNewThreadCreated(int threadId, DateTime createTime, string threadName)
    {
        CachedThreads.TryAdd(threadId, new ThreadStatusInMemoryCache
        {
            ThreadId = threadId,
            LastMessage = null,
            UserInfo = new ConcurrentDictionary<string, CachedUserInThreadInfo>(),
            ThreadCreatedTime = createTime,
            ThreadName = threadName
        });

        // Update the sorted linked list.
        ThreadIdsSortedByLastMessageTimeLock.EnterWriteLock();
        try
        {
            // Add the thread to the front.
            ThreadIdsSortedByLastMessageTime.AddFirst(threadId);
        }
        finally
        {
            ThreadIdsSortedByLastMessageTimeLock.ExitWriteLock();
        }
    }
    
    public int[] GetMyThreadIdsOrderedByLastMessageTimeDesc(string userId, int? skipTillThreadId, int take)
    {
        ThreadIdsSortedByLastMessageTimeLock.EnterReadLock();
        try
        {
            return ThreadIdsSortedByLastMessageTime
                .SkipUntilEquals(skipTillThreadId)
                .Select(tId => CachedThreads[tId])
                .Where(t => t.IsUserInThread(userId))
                .Take(take)
                .Select(t => t.ThreadId)
                .ToArray();
        }
        finally
        {
            ThreadIdsSortedByLastMessageTimeLock.ExitReadLock();
        }
    }
    
    public CachedUserInThreadInfo[] GetUsersInThread(int threadId)
    {
        return CachedThreads[threadId].GetUsersInThread();
    }

    public ThreadStatusInMemoryCache GetThreadCache(int threadId)
    {
        return CachedThreads[threadId];
    }
    
    /// <summary>
    /// This should be called when a thread is deleted.
    /// </summary>
    /// <param name="threadId"></param>
    public void OnThreadDropped(int threadId)
    {
        CachedThreads.TryRemove(threadId, out _);

        // Update the sorted linked list.
        ThreadIdsSortedByLastMessageTimeLock.EnterWriteLock();
        try
        {
            ThreadIdsSortedByLastMessageTime.Remove(threadId);
        }
        finally
        {
            ThreadIdsSortedByLastMessageTimeLock.ExitWriteLock();
        }
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
    
    // TODO: Some methods works as methods. Some methods just returns the CachedThreads[tId]. Refactor them to be consistent.
    public void OnUserJoinedThread(int threadId, string userId)
    {
        CachedThreads[threadId].OnUserJoined(userId);
    }

    public void OnUserLeftThread(int threadId, string userId)
    {
        CachedThreads[threadId].OnUserLeft(userId);
    }
    
    public void SetUserMutedStatus(int threadId, string userId, bool muted)
    {
        CachedThreads[threadId].SetUserMutedStatus(userId, muted);
    }

    public async Task PersistUserUnreadAmount()
    {
        logger.LogInformation("Persisting user unread amount to the database...");
        var threads = CachedThreads.Values.ToArray();
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<KahlaRelationalDbContext>();

        var allThreadsInDb = await dbContext
            .ChatThreads
            .Include(t => t.Members)
            .ToListAsync();

        var updatedUsersCount = 0;
        foreach (var thread in threads)
        {
            var threadId = thread.ThreadId;
            var threadInDb = allThreadsInDb.FirstOrDefault(t => t.Id == threadId);

            if (threadInDb == null)
            {
                continue;
            }

            foreach (var relation in threadInDb.Members)
            {
                var unRead = thread.GetUserUnReadAmount(relation.UserId);
                var totalMessages = arrayDbContext.GetTotalMessagesCount(threadId);
                var newReadMessageIndex = totalMessages - (int)unRead;

                if (relation.ReadMessageIndex != newReadMessageIndex)
                {
                    updatedUsersCount++;
                    relation.ReadMessageIndex = newReadMessageIndex;
                    logger.LogInformation(
                        "Persisting user unread amount for user with ID {UserId} in thread with ID {ThreadId}. His un-read message count is {UnReadMessages}. Index in database archived is {ReadMessageIndex}.",
                        relation.UserId, threadId, unRead, relation.ReadMessageIndex);
                }
            }
        }
        logger.LogInformation("Persisted user unread amount to the database. {UpdatedUsersCount} users' unread amount updated.", updatedUsersCount);

        await dbContext.SaveChangesAsync();
    }

    // TODO: Use this function to build an API.
    public long GetMyTotalUnreadMessages(string userId)
    {
        ThreadIdsSortedByLastMessageTimeLock.EnterReadLock();
        try
        {
            return ThreadIdsSortedByLastMessageTime
                .Select(tId => CachedThreads[tId])
                .Where(t => t.IsUserInThread(userId))
                .Sum(t => t.GetUserUnReadAmount(userId));
        }
        finally
        {
            ThreadIdsSortedByLastMessageTimeLock.ExitReadLock();
        }
    }

    public bool HasUnreadAtMeMessages(int threadId, string viewingUserId)
    {
        return CachedThreads[threadId].HasUnreadAtMeMessages(viewingUserId);
    }
}