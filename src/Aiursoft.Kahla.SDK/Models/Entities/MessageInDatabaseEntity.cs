using Aiursoft.ArrayDb.ObjectBucket;
using Aiursoft.ArrayDb.Partitions;

namespace Aiursoft.Kahla.SDK.Models.Entities;

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
    
    public Guid MessageId { get; init; } = Guid.NewGuid();
    public string Content { get; init; } = string.Empty;
    public DateTime SendTime { get; init; } = DateTime.UtcNow;

    public string SenderId { get; init; } = string.Empty;
}