using System.Text;
using Aiursoft.ArrayDb.ObjectBucket;
using Aiursoft.ArrayDb.ObjectBucket.Attributes;
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
    
    public Guid SenderId { get; init; } = Guid.Empty;
    
    public Guid Id { get; init; } = Guid.Empty;
    
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
    };
    
    public Commit<ChatMessage> ToCommit()
    {
        return new Commit<ChatMessage>
        {
            Item = new ChatMessage
            {
                Content = Content,
                Preview = Encoding.UTF8.GetString(Preview.TrimEndZeros()),
                SenderId = SenderId
            },
            Id = Id.ToString("D"),
            CommitTime = CreationTime
        };
    }

    public KahlaMessageMappedSentView ToSentView(KahlaUser? sender)
    {
        return new KahlaMessageMappedSentView
        {
            Id = Id,
            ThreadId = ThreadId,
            Preview = Encoding.UTF8.GetString(bytes: Preview.TrimEndZeros()),
            SendTime = CreationTime,
            Sender = sender == null ? null : new KahlaUserMappedPublicView
            {
                Id = sender.Id,
                NickName = sender.NickName,
                Bio = sender.Bio,
                IconFilePath = sender.IconFilePath,
                AccountCreateTime = sender.AccountCreateTime,
                EmailConfirmed = sender.EmailConfirmed,
                Email = sender.Email
            }
        };
    }
}