using Aiursoft.Handler.Models;
using System.Collections.Generic;

namespace Kahla.SDK.Models.ApiViewModels
{
    public class SearchEverythingViewModel : AiurProtocol
    {
        public int UsersCount { get; set; }
        public int GroupsCount { get; set; }
        public List<KahlaUser> Users { get; set; }
        public List<SearchedGroup> Groups { get; set; }
    }
}
