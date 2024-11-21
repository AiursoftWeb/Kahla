using System.Text;
using Aiursoft.ArrayDb.ObjectBucket;
using Aiursoft.ArrayDb.ObjectBucket.Attributes;
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
    
    [FixedLengthString(BytesLength = 100)]
    public byte[] Preview { get; init; } = [];
    
    public string Content { get; init; } = string.Empty;
    
    public Guid SenderId { get; init; } = Guid.Empty;
    
    public Guid Id { get; init; } = Guid.Empty;
    
    public ChatMessage ToClientView()
    {
        return new ChatMessage
        {
            Content = Content,
            Preview = Encoding.UTF8.GetString(Preview.TrimEndZeros()),
            SenderId = SenderId
        };
    }
}