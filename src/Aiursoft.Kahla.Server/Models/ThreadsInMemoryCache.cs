using System.Collections.Concurrent;
using Aiursoft.Kahla.SDK.Models.Mapped;

namespace Aiursoft.Kahla.Server.Models;

/// <summary>
/// The ThreadsInMemoryCache class is responsible for providing an in-memory caching layer to efficiently track
/// and manage message threads and users' unread message counts. It combines database initialization data with
/// real-time message tracking to ensure accurate unread message counts without relying solely on the database for
/// every query.
/// 
/// ### Essential Properties and Methods:
/// - `LastMessage`: Gets or sets the last message in the thread.
/// - `AppendedMessageSinceBootCount`: Returns the count of new messages appended to the thread since the application started.
/// - `UserUnReadAmountSinceBoot`: Gets the dictionary that stores users' unread message count since application boot.
/// 
/// ### Functions for Unread Message Count Calculation:
/// 
/// - `GetUserUnReadAmountSinceBoot(string userId)`: Retrieves the unread message count for a specific user or handles
/// unknown users by returning 0 subtracted by the appended message count.
/// - `ClearUserUnReadAmountSinceBoot(string userId)`: Clears the unread message count for a user by updating it to 0
/// subtracted by the appended message count.
/// - `AppendMessage()`: Increments the count of new messages appended to the thread since the application started.
/// </summary>
public class ThreadsInMemoryCache
{
    // Every time a message is appended to this thread, this count will increase.
    // ReSharper disable once RedundantDefaultMemberInitializer
    private uint _appendedMessageSinceBootCount = 0;
    public required int ThreadId { get; init; }

    public required KahlaMessageMappedSentView? LastMessage { get; set; }

    public required ConcurrentDictionary<string, int> UserUnReadAmountSinceBoot { private get; init; }

    public required DateTime ThreadCreatedTime { get; init; }

    public uint GetUserUnReadAmount(string userId)
    {
        // It's possible that the user is not in the thread when the app is booting.
        // If found unknown user, return 0 - appended message count.
        var unread = UserUnReadAmountSinceBoot.GetOrAdd(userId, _ => 0 - (int)_appendedMessageSinceBootCount) + _appendedMessageSinceBootCount;
        return unread < 0 ? 0 : (uint)unread;
    }

    public void ClearUserUnReadAmountSinceBoot(string userId)
    {
        if (UserUnReadAmountSinceBoot.ContainsKey(userId))
        {
            UserUnReadAmountSinceBoot[userId] = 0 - (int)_appendedMessageSinceBootCount;
        }
        else
        {
            UserUnReadAmountSinceBoot.TryAdd(userId, 0 - (int)_appendedMessageSinceBootCount);
        }
    }

    public void AppendMessagesCount(uint messagesCount)
    {
        //Interlocked.Increment(ref _appendedMessageSinceBootCount);
        Interlocked.Add(ref _appendedMessageSinceBootCount, messagesCount);
    }

    public void OnUserJoined(string userId)
    {
        UserUnReadAmountSinceBoot.TryAdd(userId, 0 - (int)_appendedMessageSinceBootCount);
    }

    public void OnUserLeft(string userId)
    {
        // TODO: Call this function.
        UserUnReadAmountSinceBoot.TryRemove(userId, out _);
    }

    public bool IsUserInThread(string userId)
    {
        return UserUnReadAmountSinceBoot.ContainsKey(userId);
    }
    
    public string[] GetUsersInThread()
    {
        return UserUnReadAmountSinceBoot.Keys.ToArray();
    }
}