namespace Aiursoft.Kahla.SDK.Models.Mapped;

public class KahlaUserMappedInThreadView : KahlaUserMappedOthersView
{
    public bool IsAdmin { get; set; }
    
    public bool IsOwner { get; set; }
    
    public DateTime JoinTime { get; set; }
}