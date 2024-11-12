using System.Collections.Concurrent;
using Aiursoft.AiurObserver;
using Aiursoft.Kahla.Server.Models.Entities;

namespace Aiursoft.Kahla.Server.Data;

public class ChannelsInMemoryDb
{
    private ConcurrentDictionary<string, AsyncObservable<string>> UserListenChannels { get; } = new();
    private ConcurrentDictionary<int, AsyncObservable<MessageInDatabaseEntity[]>> ThreadsListenChannels { get; } = new();

    public AsyncObservable<string> GetMyChannel(string userId)
    {
        lock (UserListenChannels)
        {
            return UserListenChannels.GetOrAdd(userId, _ => new AsyncObservable<string>());
        }
    }
    
    public AsyncObservable<MessageInDatabaseEntity[]> GetThreadNewMessagesChannel(int threadId)
    {
        lock (ThreadsListenChannels)
        {
            return ThreadsListenChannels.GetOrAdd(threadId, _ => new AsyncObservable<MessageInDatabaseEntity[]>());
        }
    }
}