using InfluxDB.Client.Core;

namespace Aiursoft.Kahla.Server.Data.Models;

public class MessageInfluxEntity
{
    [Column(IsTimestamp = true)] public required DateTime Time { get; init; }

    [Column(nameof(MessageId))] public required Guid MessageId { get; init; }

    [Column(nameof(SenderId), IsTag = true)] public required Guid SenderId { get; init; }

    [Column(nameof(ThreadId), IsTag = true)] public required int ThreadId { get; init; }

    [Column(nameof(Content))] public required string Content { get; init; }
}