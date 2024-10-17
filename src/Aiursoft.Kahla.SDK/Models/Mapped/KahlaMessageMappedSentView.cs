using Aiursoft.Kahla.SDK.Models.Entities;

namespace Aiursoft.Kahla.SDK.Models.Mapped;

public class KahlaMessageMappedSentView
{
    public required Guid Id { get; init; }
    public required int ThreadId { get; init; }
    public required KahlaUser? Sender { get; init; }
    public required string Content { get; init; }
    public DateTime SendTime { get; init; }
}