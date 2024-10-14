using Aiursoft.Kahla.SDK.Models.Entities;

namespace Aiursoft.Kahla.SDK.Models.Mapped;

public class KahlaUserMappedOthersView
{
    public required KahlaUser User { get; init; }
    
    public required bool? Online { get; init; }
    
    public required bool IsKnownContact { get; init; }
    
    public required bool IsBlockedByYou { get; init; }
}