using System.Collections.Generic;
using Aiursoft.Pylon.Models;

namespace Kahla.Server.Models.ApiViewModels
{
    public class SearchEverythingViewModel : AiurProtocol
    {
        public List<KahlaUser> Users { get; set; }
        public List<GroupConversation> Groups { get; set; }
    }
}
