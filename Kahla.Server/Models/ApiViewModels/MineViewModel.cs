using Aiursoft.Pylon.Models;
using System.Collections.Generic;

namespace Kahla.Server.Models.ApiViewModels
{
    public class MineViewModel : AiurProtocol
    {
        public List<KahlaUser> Users { get; internal set; }
        public List<SearchedGroup> Groups { get; internal set; }
    }
}
