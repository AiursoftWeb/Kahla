using Aiursoft.InMemoryKvDb.AutoCreate;

namespace Aiursoft.Kahla.Server.Data;

public class LocksInMemoryDb(NamedLruMemoryStoreProvider<SemaphoreSlim, string> memoryStoreProvider)
{
    public SemaphoreSlim GetFriendsOperationLock(string lockId)
    {
        return memoryStoreProvider.GetStore("FriendsOperationLocks").GetOrAdd(lockId);
    }
    
    public SemaphoreSlim GetBlockOperationLock(string lockId)
    {
        return memoryStoreProvider.GetStore("BlockOperationLocks").GetOrAdd(lockId);
    }
    
    public SemaphoreSlim GetJoinThreadOperationLock(string userId, int threadId)
    {
        return memoryStoreProvider.GetStore("JoinThreadOperationLocks").GetOrAdd($"thread-join-{userId}-{threadId}");
    }
}