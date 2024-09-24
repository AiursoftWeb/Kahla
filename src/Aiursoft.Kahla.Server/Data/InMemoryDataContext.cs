using System.Collections.Concurrent;
using Aiursoft.AiurObserver;

namespace Aiursoft.Kahla.Server.Data;

public class InMemoryDataContext
{
    public ConcurrentDictionary<string, AsyncObservable<string>> UserListenChannels { get; } = new();
    
    public AsyncObservable<string> GetMyChannel(string userId)
    {
        lock (UserListenChannels)
        {
            return UserListenChannels.GetOrAdd(userId, _ => new AsyncObservable<string>());
        }
    }
}