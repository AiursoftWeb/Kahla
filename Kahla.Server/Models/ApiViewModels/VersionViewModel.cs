using Aiursoft.Pylon.Models;

namespace Kahla.Server.Models.ApiViewModels
{
    public class VersionViewModel : AiurProtocol
    {
        public string LatestVersion { get; set; }
        public string OldestSupportedVersion { get; set; }
        public string DownloadAddress { get; set; }
    }
}
