using Aiursoft.AiurProtocol;
using Aiursoft.AiurProtocol.Services;
using Aiursoft.Kahla.SDK.Models.ApiViewModels;
using Aiursoft.Scanner.Abstractions;

namespace Aiursoft.Kahla.SDK.Services
{
    public class HomeService : IScopedDependency
    {
        private readonly AiurProtocolClient _http;

        public HomeService(AiurProtocolClient http)
        {
            _http = http;
        }

        public async Task<IndexViewModel> IndexAsync(string serverRoot)
        {
            var url = new AiurApiEndpoint(serverRoot, "Home", "Index", new { });
            return await _http.Get<IndexViewModel>(url);
        }
    }
}
