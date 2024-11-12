using Aiursoft.ArrayDb.ObjectBucket;
using Aiursoft.ArrayDb.Partitions;
using Aiursoft.Kahla.SDK.Models;

namespace Aiursoft.Kahla.Server.Models.Entities;

public class MessageInDatabaseEntity : PartitionedBucketEntity<int>
{
    [PartitionKey] 
    public int ThreadId { get; set; }

    [PartitionKey]
    public override int PartitionId
    {
        get => ThreadId;
        set => ThreadId = value;
    }
    
    public string Content { get; init; } = string.Empty;
    
    // TODO: Replace to type: Guid
    public string SenderId { get; init; } = string.Empty;
    
    // TODO: Replace to type: Guid
    public string Id { get; init; } = Guid.Empty.ToString("D");
    
    public ChatMessage ToClientView()
    {
        return new ChatMessage
        {
            Content = Content,
            SenderId = SenderId
        };
    }
}