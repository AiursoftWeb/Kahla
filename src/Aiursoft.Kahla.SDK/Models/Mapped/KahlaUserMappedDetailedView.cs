namespace Aiursoft.Kahla.SDK.Models.Mapped;

public class KahlaUserMappedDetailedView
{
    public required KahlaUserMappedOthersView SearchedUser { get; init; }
    public required List<KahlaThreadMappedJoinedView> CommonThreads { get; init; } = new();
}