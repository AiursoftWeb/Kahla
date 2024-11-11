using Aiursoft.InMemoryKvDb.AutoCreate;

namespace Aiursoft.Kahla.Server.Data;

public class LocksDb(NamedLruMemoryStoreProvider<SemaphoreSlim, string> memoryStoreProvider)
{
    public SemaphoreSlim GetFriendsOperationLock(string lockId)
    {
        return memoryStoreProvider.GetStore("FriendsOperationLocks").GetOrAdd(lockId);
    }
    
    public SemaphoreSlim GetBlockOperationLock(string lockId)
    {
        return memoryStoreProvider.GetStore("BlockOperationLocks").GetOrAdd(lockId);
    }
}