using System.Collections.Concurrent;
using Aiursoft.Kahla.SDK.Models.Mapped;

namespace Aiursoft.Kahla.Server.Models;

/// <summary>
/// The ThreadsInMemoryCache class is responsible for providing an in-memory caching layer to efficiently track
/// and manage message threads and users' unread message counts, as well as their mute status. It combines database
/// initialization data with real-time message tracking to ensure accurate unread message counts without relying
/// solely on the database for every query.
/// 
/// ### Essential Properties and Methods:
/// - `LastMessage`: Gets or sets the last message in the thread.
/// - `AppendedMessageSinceBootCount`: Returns the count of new messages appended to the thread since the application started.
/// - `UserInfo`: Gets the dictionary that stores users' unread message count and mute status since application boot.
/// 
/// ### Functions for Unread Message Count and Mute Status Calculation:
/// 
/// - `GetUserUnReadAmount(string userId)`: Retrieves the unread message count for a specific user or handles
/// unknown users by returning 0 subtracted by the appended message count.
/// - `ClearUserUnReadAmountSinceBoot(string userId)`: Clears the unread message count for a user by updating it to 0
/// subtracted by the appended message count.
/// - `GetUserMutedStatus(string userId)`: Retrieves the mute status for a specific user.
/// - `SetUserMutedStatus(string userId, bool muted)`: Sets the mute status for a specific user.
/// - `AppendMessagesCount(uint messagesCount)`: Increments the count of new messages appended to the thread since the application started.
/// </summary>
public class ThreadStatusInMemoryCache
{
    // Every time a message is appended to this thread, this count will increase.
    // ReSharper disable once RedundantDefaultMemberInitializer
    private uint _appendedMessageSinceBootCount = 0;
    public required int ThreadId { get; init; }

    public required KahlaMessageMappedSentView? LastMessage { get; set; }

    public required ConcurrentDictionary<string, CachedUserInThreadInfo> UserInfo { private get; init; }
    
    public required DateTime ThreadCreatedTime { get; init; }
    
    public required string ThreadName { get; set; }

    public uint GetUserUnReadAmount(string userId)
    {
        if (UserInfo.TryGetValue(userId, out var cached))
        {
            var unread = cached.UnreadAmountSinceBoot + _appendedMessageSinceBootCount;
            return unread < 0 ? 0 : (uint)unread;
        }
        else
        {
            throw new InvalidOperationException($"While getting user unread amount, user {userId} not found in the thread!");
        }
    }

    public void ClearUserUnReadAmountSinceBoot(string userId)
    {
        if (UserInfo.TryGetValue(userId, out var cached))
        {
            cached.UnreadAmountSinceBoot = 0 - (int)_appendedMessageSinceBootCount;
        }
        else
        {
            throw new InvalidOperationException($"While clearing user unread amount, user {userId} not found in the thread!");
        }
    }

    public void SetUserMutedStatus(string userId, bool muted)
    {
        if (UserInfo.TryGetValue(userId, out var cached))
        {
            cached.Muted = muted;
        }
        else
        {
            throw new InvalidOperationException($"While setting user mute status, user {userId} not found in the thread!");
        }
    }

    public void AppendMessagesCount(uint messagesCount)
    {
        Interlocked.Add(ref _appendedMessageSinceBootCount, messagesCount);
    }

    public void OnUserJoined(string userId)
    {
        UserInfo.TryAdd(userId, new CachedUserInThreadInfo
        {
            UserId = userId,
            UnreadAmountSinceBoot = 0 - (int)_appendedMessageSinceBootCount, 
            Muted = false,
            BeingAted = false
        });
    }

    public void OnUserLeft(string userId)
    {
        UserInfo.TryRemove(userId, out _);
    }

    public bool IsUserInThread(string userId)
    {
        return UserInfo.ContainsKey(userId);
    }

    public CachedUserInThreadInfo[] GetUsersInThread()
    {
        return UserInfo.Values.ToArray();
    }

    public bool HasUnreadAtMeMessages(string viewingUserId)
    {
        if (UserInfo.TryGetValue(viewingUserId, out var cached))
        {
            return cached.BeingAted;
        }
        else
        {
            throw new InvalidOperationException($"While checking user at status, user {viewingUserId} not found in the thread with ID: {ThreadId}!");
        }
    }
    
    public void AtUser(string userId)
    {
        if (UserInfo.TryGetValue(userId, out var cached))
        {
            cached.BeingAted = true;
        }
        else
        {
            throw new InvalidOperationException($"While setting user at status, user {userId} not found in the thread with ID: {ThreadId}!");
        }
    }
    
    public void ClearAtForUser(string userId)
    {
        if (UserInfo.TryGetValue(userId, out var cached))
        {
            cached.BeingAted = false;
        }
        else
        {
            throw new InvalidOperationException($"While clearing user at status, user {userId} not found in the thread!");
        }
    }
}

public class CachedUserInThreadInfo
{
    public required string UserId { get; init; }
    public required int UnreadAmountSinceBoot { get; set; }
    public required bool Muted { get; set; }
    
    public required bool BeingAted { get; set; }
}
