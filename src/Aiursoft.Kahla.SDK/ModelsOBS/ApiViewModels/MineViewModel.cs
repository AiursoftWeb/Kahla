using Aiursoft.AiurProtocol.Models;
using Aiursoft.Kahla.SDK.Models;

namespace Aiursoft.Kahla.SDK.ModelsOBS.ApiViewModels;

[Obsolete]
public class MineViewModel : AiurResponse
{
    public IEnumerable<KahlaUserWithOnlineStatus> Users { get; set; } = new List<KahlaUserWithOnlineStatus>();
    public IEnumerable<SearchedGroup> Groups { get; set; } = new List<SearchedGroup>();
}

public class KahlaUserWithOnlineStatus
{
    public required KahlaUser User { get; init; }
    
    public bool? Online { get; set; }
}