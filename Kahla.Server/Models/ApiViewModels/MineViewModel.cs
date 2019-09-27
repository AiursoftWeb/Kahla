using Aiursoft.Pylon.Models;
using System.Collections.Generic;

namespace Kahla.Server.Models.ApiViewModels
{
    public class MineViewModel : AiurProtocol
    {
        public IEnumerable<KahlaUser> Users { get; internal set; }
        public IEnumerable<SearchedGroup> Groups { get; internal set; }
    }
}
