using Aiursoft.ArrayDb.Partitions;
using Aiursoft.Kahla.SDK.Models.Entities;

namespace Aiursoft.Kahla.Server.Data;

public class ArrayDbContext(PartitionedObjectBucket<MessageInDatabaseEntity, int> bucket)
{
    public MessageInDatabaseEntity? GetLastMessage(int threadId)
    {
        var count = bucket.Count(threadId);
        if (count == 0)
        {
            return null;
        }
        
        // TODO: Refactor the SDK.
#pragma warning disable CS0618 // Type or member is obsolete
        var lastMessage = bucket.Read(partitionKey: threadId, index: count - 1);
#pragma warning restore CS0618 // Type or member is obsolete
        return lastMessage;
    }
    
    public int GetTotalMessagesCount(int threadId)
    {
        return bucket.Count(threadId);
    }
    
    public void AddMessage(MessageInDatabaseEntity message)
    {
        bucket.Add(message);
    }
    
    public async Task DeleteThreadAsync(int threadId)
    {
        await bucket.DeletePartitionAsync(threadId);
    }
    
    public void CreateNewThread(int threadId)
    {
        var newThread = bucket.GetPartitionById(threadId);
        var zeroMessagesCount = newThread.InnerBucket.ArchivedItemsCount;
        if (zeroMessagesCount != 0)
        {
            throw new InvalidOperationException("The thread should be empty when created!");
        }
    }
}