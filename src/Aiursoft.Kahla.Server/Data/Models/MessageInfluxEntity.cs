using InfluxDB.Client.Core;

namespace Aiursoft.Kahla.Server.Data.Models;

public class MessageInfluxInsertingEntity
{
    public required Guid MessageId { get; init; }

    public required Guid SenderId { get; init; }

    public required int ThreadId { get; init; }
    
    public required DateTime SendTime { get; init; }

    public required string InnerContent { get; init; }

    public string ToInfluxField()
    {
        return $"{MessageId}|{InnerContent}";
    }
}

[Measurement(nameof(MessageInfluxInsertingEntity))]
public class MessageInfluxReadingEntity
{
    [Column(nameof(MessageInfluxInsertingEntity.ThreadId), IsTag = true)]
    public required int ThreadId { get; init; }
    
    [Column(nameof(MessageInfluxInsertingEntity.SenderId), IsTag = true)]
    public required string SenderId { get; init; }

    [Column("_value")]
    public required string Content { get; init; }

    [Column(IsTimestamp = true)] // 时间戳
    public required DateTime Time { get; init; }
}