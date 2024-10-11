namespace Aiursoft.Kahla.SDK.Models.Mapped;

public class KahlaUserMappedDetailedOthersView : KahlaUserMappedOthersView
{
    public List<KahlaThreadMappedJoinedView> CommonThreads { get; init; } = new();
    
    public bool IsKnownContact { get; set; }
    
    public bool IsBlockedByYou { get; set; }
}