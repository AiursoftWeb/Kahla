using Aiursoft.AiurProtocol.Models;
using Aiursoft.Kahla.SDK.Models.Mapped;

namespace Aiursoft.Kahla.SDK.Models.ViewModels;

public class SearchThreadsViewModel : AiurResponse
{
    public required int TotalThreadsCount { get; init; }
    public required List<KahlaThreadMappedOthersView> Threads { get; init; }
}