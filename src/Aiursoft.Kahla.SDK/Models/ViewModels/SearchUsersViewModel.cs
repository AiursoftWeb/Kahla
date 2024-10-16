using Aiursoft.AiurProtocol.Models;
using Aiursoft.Kahla.SDK.Models.Mapped;

namespace Aiursoft.Kahla.SDK.Models.ViewModels;

public class SearchUsersViewModel : AiurResponse
{
    public required int TotalUsersCount { get; init; }
    public required List<KahlaUserMappedOthersView> Users { get; init; }
}