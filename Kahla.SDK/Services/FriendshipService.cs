using Aiursoft.Handler.Exceptions;
using Aiursoft.Handler.Models;
using Aiursoft.Scanner.Interfaces;
using Aiursoft.XelNaga.Models;
using Kahla.SDK.Models;
using Kahla.SDK.Models.ApiAddressModels;
using Kahla.SDK.Models.ApiViewModels;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Kahla.SDK.Services
{
    public class FriendshipService : IScopedDependency
    {
        private readonly KahlaLocation _kahlaLocation;
        private readonly SingletonHTTP _http;

        public FriendshipService(
            KahlaLocation kahlaLocation,
            SingletonHTTP http)
        {
            _kahlaLocation = kahlaLocation;
            _http = http;
        }

        public async Task<MineViewModel> MineAsync()
        {
            var url = new AiurUrl(_kahlaLocation.ToString(), "Friendship", "Mine", new { });
            var result = await _http.Get(url);
            var jsonResult = JsonConvert.DeserializeObject<MineViewModel>(result);

            if (jsonResult.Code != ErrorType.Success)
                throw new AiurUnexceptedResponse(jsonResult);

            return jsonResult;
        }

        public async Task CompleteRequestAsync(int requestId, bool accept)
        {
            var url = new AiurUrl(_kahlaLocation.ToString(), "Friendship", "CompleteRequest", new { });
            var form = new AiurUrl(string.Empty, new CompleteRequestAddressModel
            {
                Id = requestId,
                Accept = accept
            });
            var result = await _http.Post(url, form);
            var jsonResult = JsonConvert.DeserializeObject<AiurValue<AiurProtocol>>(result);

            if (jsonResult.Code != ErrorType.Success)
                throw new AiurUnexceptedResponse(jsonResult);
        }

        public async Task<AiurCollection<Request>> MyRequestsAsync()
        {
            var url = new AiurUrl(_kahlaLocation.ToString(), "Friendship", "MyRequests", new { });
            var result = await _http.Get(url);
            var jsonResult = JsonConvert.DeserializeObject<AiurCollection<Request>>(result);

            if (jsonResult.Code != ErrorType.Success)
                throw new AiurUnexceptedResponse(jsonResult);

            return jsonResult;
        }
    }
}
