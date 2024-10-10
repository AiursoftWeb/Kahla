namespace Aiursoft.Kahla.SDK.Models.Mapped;

public class KahlaUserMappedOthersView
{
    public required KahlaUser User { get; init; }
    
    public bool? Online { get; set; }
}