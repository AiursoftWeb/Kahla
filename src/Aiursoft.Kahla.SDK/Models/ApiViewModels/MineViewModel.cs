using Aiursoft.AiurProtocol;
using Aiursoft.AiurProtocol.Models;

namespace Aiursoft.Kahla.SDK.Models.ApiViewModels
{
    public class MineViewModel : AiurResponse
    {
        public IEnumerable<KahlaUser> Users { get; set; }
        public IEnumerable<SearchedGroup> Groups { get; set; }
    }
}
