using Aiursoft.AiurProtocol.Models;
using Aiursoft.Kahla.SDK.Models.Mapped;

namespace Aiursoft.Kahla.SDK.Models.ViewModels;

public class SearchUsersViewModel : AiurResponse
{
    public required int TotalUsersCount { get; set; }
    public required List<KahlaUserMappedOthersView> Users { get; init; } = new();
}

public class SearchThreadsViewModel : AiurResponse
{
    public required int TotalThreadsCount { get; set; }
    public required List<KahlaThreadMappedOthersView> Threads { get; set; } = new();
}