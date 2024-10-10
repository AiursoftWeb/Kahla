using Aiursoft.AiurProtocol.Models;

namespace Aiursoft.Kahla.SDK.Models.Mapped;

public class KahlaUserMappedOthersView
{
    public required KahlaUser User { get; init; }
    
    public bool? Online { get; set; }
}

public class KahlaUserMappedOthersViewWithCommonThreads : KahlaUserMappedOthersView
{
    public List<KahlaThreadMappedJoinedView> CommonThreads { get; init; } = new();
}

public class UserDetailViewModel : AiurResponse
{
    public required KahlaUserMappedOthersViewWithCommonThreads User { get; init; }
}