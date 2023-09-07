using Aiursoft.Scanner.Abstractions;
using Kahla.SDK.Models.ApiViewModels;
using Newtonsoft.Json;

namespace Kahla.SDK.Services
{
    public class HomeService : IScopedDependency
    {
        private readonly HttpService _http;

        public HomeService(HttpService http)
        {
            _http = http;
        }

        public async Task<IndexViewModel> IndexAsync(string serverRoot)
        {
            var url = new AiurUrl(serverRoot, "Home", "Index", new { });
            var result = await _http.Get(url);
            var jResult = JsonConvert.DeserializeObject<IndexViewModel>(result);

            if (jResult.Code != ErrorType.Success)
                throw new AiurUnexpectedResponse(jResult);
            return jResult;
        }
    }
}
