namespace Aiursoft.Kahla.SDK.Models.Mapped;

public class KahlaUserMappedInThreadView : KahlaUserMappedOthersView
{
    public required bool IsAdmin { get; init; }
    
    public required bool IsOwner { get; init; }
    
    public required DateTime JoinTime { get; init; }
}