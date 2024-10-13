using Aiursoft.AiurProtocol.Models;
using Aiursoft.Kahla.SDK.Models.Mapped;

namespace Aiursoft.Kahla.SDK.Models.ViewModels
{
    public class SearchEverythingViewModel : AiurResponse
    {
        public required int TotalUsersCount { get; set; }
        public required int TotalThreadsCount { get; set; }
        public required List<KahlaUserMappedOthersView> Users { get; init; } = new();
        public required List<KahlaThreadMappedOthersView> Threads { get; set; } = new();
    }
}
