using Aiursoft.AiurProtocol.Models;
using Aiursoft.Kahla.SDK.Models.Mapped;

namespace Aiursoft.Kahla.SDK.Models.ViewModels;

public class UserDetailViewModel : AiurResponse
{
    public required KahlaUserMappedOthersView SearchedUser { get; init; }
    public required List<KahlaThreadMappedJoinedView> CommonThreads { get; init; } = new();
    public required int CommonThreadsCount { get; set; }
}