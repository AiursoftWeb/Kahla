using System.Text;
using Aiursoft.ArrayDb.ObjectBucket;
using Aiursoft.ArrayDb.ObjectBucket.Abstractions.Attributes;
using Aiursoft.ArrayDb.Partitions;
using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Models.Mapped;
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
    
    public string AtsStored { get; init; } = string.Empty;
    
    public Guid SenderId { get; init; } = Guid.Empty;
    
    public Guid Id { get; init; } = Guid.Empty;
    
    public DateTime CreationTime { get; init; } = DateTime.UtcNow;

    public Guid[] GetAtsAsGuids()
    {
        return AtsStored
            .Split(',')
            .Select(Convert.FromBase64String)
            .Select(bytes => new Guid(bytes))
            .ToArray();
    }
    
    public static MessageInDatabaseEntity FromPushedCommit(
        Commit<ChatMessage> messageIncoming, 
        DateTime serverTime,
        Guid userIdGuid) => new()
    {
        Content = messageIncoming.Item.Content,
        Preview = Encoding.UTF8.GetBytes(messageIncoming.Item.Preview).Take(50).ToArray(),
        Id = Guid.Parse(messageIncoming.Id),
        CreationTime = serverTime,
        SenderId = userIdGuid,
        AtsStored = string.Join(",", messageIncoming.Item.Ats.Select(guid => Convert.ToBase64String(guid.ToByteArray()))) 
    };
    
    public Commit<ChatMessage> ToCommit()
    {
        return new Commit<ChatMessage>
        {
            Item = new ChatMessage
            {
                Content = Content,
                Preview = Encoding.UTF8.GetString(Preview.TrimEndZeros()),
                SenderId = SenderId,
                Ats = AtsStored.Split(',').Select(Convert.FromBase64String).Select(bytes => new Guid(bytes)).ToArray()
            },
            Id = Id.ToString("D"),
            CommitTime = CreationTime
        };
    }

    public KahlaMessageMappedSentView ToSentView(KahlaUserMappedPublicView? sender)
    {
        return new KahlaMessageMappedSentView
        {
            Id = Id,
            ThreadId = ThreadId,
            Preview = Encoding.UTF8.GetString(bytes: Preview.TrimEndZeros()),
            SendTime = CreationTime,
            Ats = AtsStored.Split(',').Select(Convert.FromBase64String).Select(bytes => new Guid(bytes)).ToArray(),
            Sender = sender
        };
    }
}