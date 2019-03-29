using System.Collections.Generic;
using Aiursoft.Pylon.Models;

namespace Kahla.Server.Models.ApiViewModels
{
    public class SearchEverythingViewModel : AiurProtocol
    {
        public int UsersCount { get; set; }
        public int GroupsCount { get; set; }
        public List<KahlaUser> Users { get; set; }
        public List<SearchedGroup> Groups { get; set; }
    }
}
