using Aiursoft.AiurProtocol.Models;
using Aiursoft.Kahla.SDK.Models.Mapped;

namespace Aiursoft.Kahla.SDK.Models.ViewModels
{
    public class SearchEverythingViewModel : AiurResponse
    {
        public int TotalUsersCount { get; set; }
        public int TotalThreadsCount { get; set; }
        public List<KahlaUserMappedOthersView> Users { get; set; } = new();
        public List<KahlaThreadMappedSearchedView> Threads { get; set; } = new();
    }
}
