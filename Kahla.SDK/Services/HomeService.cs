using Aiursoft.AiurProtocol;
using Aiursoft.Scanner.Abstractions;
using Kahla.SDK.Models.ApiViewModels;

namespace Kahla.SDK.Services
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
