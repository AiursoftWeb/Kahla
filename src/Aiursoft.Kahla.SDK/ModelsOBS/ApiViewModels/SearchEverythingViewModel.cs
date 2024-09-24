using Aiursoft.AiurProtocol.Models;

namespace Aiursoft.Kahla.SDK.ModelsOBS.ApiViewModels
{
    public class SearchEverythingViewModel : AiurResponse
    {
        public int UsersCount { get; set; }
        public int GroupsCount { get; set; }
        public List<KahlaUser> Users { get; set; } = new List<KahlaUser>();
        public List<SearchedGroup> Groups { get; set; } = new List<SearchedGroup>();
    }
}
