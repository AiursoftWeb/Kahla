using Aiursoft.AiurProtocol.Models;
using Aiursoft.Kahla.SDK.Models;

namespace Aiursoft.Kahla.SDK.ModelsOBS.ApiViewModels
{
    public class MineViewModel : AiurResponse
    {
        public IEnumerable<KahlaUser> Users { get; set; } = new List<KahlaUser>();
        public IEnumerable<SearchedGroup> Groups { get; set; } = new List<SearchedGroup>();
    }
}
