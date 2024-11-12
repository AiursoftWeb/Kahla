
namespace Aiursoft.Kahla.SDK.Models.Mapped;

public class KahlaMessageMappedSentView
{
    public required Guid Id { get; init; }
    public required int ThreadId { get; init; }
    public required KahlaUserMappedPublicView? Sender { get; init; }
    public required string Content { get; init; }
    public required DateTime SendTime { get; init; }
}