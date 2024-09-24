using Aiursoft.AiurProtocol.Models;

namespace Aiursoft.Kahla.SDK.Models.ApiViewModels
{
    public class IndexViewModel : AiurResponse // TODO: Use this class.
    {
        public DateTime ServerTime { get; set; }
        public DateTime UtcTime { get; set; }
        public required string ApiVersion { get; set; }
        public required string VapidPublicKey { get; set; }
        public required string ServerName { get; set; }
        public required string Mode { get; set; }
        public bool AutoAcceptRequests { get; set; }
    }
}
