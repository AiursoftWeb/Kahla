using Aiursoft.AiurProtocol.Models;

namespace Aiursoft.Kahla.SDK.Models.ViewModels
{
    public class IndexViewModel : AiurResponse
    {
        public required string VapidPublicKey { get; init; }
        public required string ServerName { get; init; }
    }
}
