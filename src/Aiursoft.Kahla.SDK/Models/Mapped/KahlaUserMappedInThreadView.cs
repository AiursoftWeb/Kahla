namespace Aiursoft.Kahla.SDK.Models.Mapped;

public class KahlaUserMappedInThreadView : KahlaUserMappedOthersView
{
    public required bool IsAdmin { get; set; }
    
    public required bool IsOwner { get; set; }
    
    public required DateTime JoinTime { get; set; }
}