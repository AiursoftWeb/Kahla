using Aiursoft.AiurProtocol.Models;

namespace Aiursoft.Kahla.SDK.Models.ApiViewModels
{
    public class VersionViewModel : AiurResponse
    {
        public string LatestVersion { get; set; }
        public string LatestCliVersion { get; set; }
        public string DownloadAddress { get; set; }
    }
}
