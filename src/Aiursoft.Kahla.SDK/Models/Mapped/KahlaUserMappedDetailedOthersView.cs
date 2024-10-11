namespace Aiursoft.Kahla.SDK.Models.Mapped;

public class KahlaUserMappedDetailedOthersView : KahlaUserMappedOthersView
{
    public List<KahlaThreadMappedJoinedView> CommonThreads { get; init; } = new();
}