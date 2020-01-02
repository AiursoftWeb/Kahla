using Aiursoft.XelNaga.Models;

namespace Kahla.SDK.Models.ApiViewModels
{
    public class VersionViewModel : AiurProtocol
    {
        public string LatestVersion { get; set; }
        public string LatestCLIVersion { get; set; }
        public string DownloadAddress { get; set; }
        public string APIVersion { get; set; }
    }
}
