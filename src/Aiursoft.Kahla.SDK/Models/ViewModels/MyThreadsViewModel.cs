using Aiursoft.AiurProtocol.Models;
using Aiursoft.Kahla.SDK.Models.Mapped;

namespace Aiursoft.Kahla.SDK.Models.ViewModels;

public class MyThreadsViewModel : AiurResponse
{
    public required int TotalCount { get; set; }
    public required List<KahlaThreadMappedJoinedView> KnownThreads { get; set; } = new();
}