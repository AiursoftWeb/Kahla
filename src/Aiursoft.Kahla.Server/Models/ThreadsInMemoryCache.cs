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
    public required KahlaMessageMappedSentView? LastMessage { get; set; }
    
    // Every time a message is appended to this thread, this count will increase.
    private uint _appendedMessageSinceBootCount;

    public required ConcurrentDictionary<string, int> UserUnReadAmountSinceBoot { private get; init; }
    
    public uint GetUserUnReadAmount(string userId)
    {
        // It's possible that the user is not in the thread when the app is booting.
        // If found unknown user, return 0 - appended message count.
        return (uint)(UserUnReadAmountSinceBoot.GetOrAdd(userId, _ => 0 - (int)_appendedMessageSinceBootCount) + _appendedMessageSinceBootCount);
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
    
    public void AppendMessage()
    {
        Interlocked.Increment(ref _appendedMessageSinceBootCount);
    }
}