using Aiursoft.XelNaga.Models;
using System.Collections.Generic;

namespace Kahla.SDK.Models.ApiViewModels
{
    public class MineViewModel : AiurProtocol
    {
        public IEnumerable<KahlaUser> Users { get; set; }
        public IEnumerable<SearchedGroup> Groups { get; set; }
    }
}
