using System.Text;
using Aiursoft.ArrayDb.ObjectBucket;
using Aiursoft.ArrayDb.ObjectBucket.Attributes;
using Aiursoft.ArrayDb.Partitions;
using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Services;

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
    
    private ChatMessage ToClientView()
    {
        return new ChatMessage
        {
            Content = Content,
            Preview = Encoding.UTF8.GetString(Preview.TrimEndZeros()),
            SenderId = SenderId
        };
    }
    
    public Commit<ChatMessage> ToCommit()
    {
        return new Commit<ChatMessage>
        {
            Item = ToClientView(),
            Id = Id.ToString("D"),
            CommitTime = CreationTime
        };
    }

    public static MessageInDatabaseEntity FromClientPushedCommit(
        Commit<ChatMessage> messageIncoming, 
        DateTime serverTime,
        Guid userIdGuid) => new()
    {
        Content = messageIncoming.Item.Content,
        Preview = Encoding.UTF8.GetBytes(messageIncoming.Item.Preview).Take(50).ToArray(),
        Id = Guid.Parse(messageIncoming.Id),
        CreationTime = serverTime,
        SenderId = userIdGuid,
    };
}