using Aiursoft.Probe.SDK.Models.HomeViewModels;

namespace Kahla.SDK.Models.ApiViewModels
{
    public class IndexViewModel : AiurProtocol
    {
        public string WikiPath { get; set; }
        public DateTime ServerTime { get; set; }
        public DateTime UTCTime { get; set; }
        public string APIVersion { get; set; }
        public string VapidPublicKey { get; set; }
        public string ServerName { get; set; }
        public string Mode { get; set; }
        public DomainSettings Domain { get; set; }
        public ProbeDownloadPatternConfig Probe { get; set; }
        public bool AutoAcceptRequests { get; set; }
    }
}
