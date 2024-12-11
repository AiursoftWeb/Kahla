using Aiursoft.ArrayDb.Partitions;
using Aiursoft.Kahla.Server.Models.Entities;

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
        
        var lastMessage = bucket.Read(partitionKey: threadId, index: count - 1);
        return lastMessage;
    }
    
    public MessageInDatabaseEntity[] ReadBulk(int threadId, int start, int count)
    {
        return count == 0 ? [] : bucket.ReadBulk(threadId, start, count);
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
        var zeroMessagesCount = newThread.Count;
        if (zeroMessagesCount != 0)
        {
            throw new InvalidOperationException("The thread should be empty when created!");
        }
    }
}