using Aiursoft.AiurProtocol.Models;

namespace Aiursoft.Kahla.SDK.Models.ApiViewModels
{
    public class IndexViewModel : AiurResponse
    {
        public string WikiPath { get; set; }
        public DateTime ServerTime { get; set; }
        public DateTime UtcTime { get; set; }
        public string ApiVersion { get; set; }
        public string VapidPublicKey { get; set; }
        public string ServerName { get; set; }
        public string Mode { get; set; }
        public bool AutoAcceptRequests { get; set; }
    }
}
