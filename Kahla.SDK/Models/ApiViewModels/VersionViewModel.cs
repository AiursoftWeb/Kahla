namespace Kahla.SDK.Models.ApiViewModels
{
    public class VersionViewModel : AiurProtocol
    {
        public string LatestVersion { get; set; }
        public string LatestCLIVersion { get; set; }
        public string DownloadAddress { get; set; }
    }
}
