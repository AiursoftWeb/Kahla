using Aiursoft.Pylon.Exceptions;
using Aiursoft.Pylon.Interfaces;
using Aiursoft.Pylon.Models;
using Kahla.SDK.Models.ApiAddressModels;
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

        public async Task CompleteRequestAsync(int requestId, bool accept)
        {
            var url = new AiurUrl(_kahlaLocation.ToString(), "Friendship", "CompleteRequest", new { });
            var form = new AiurUrl(string.Empty, new CompleteRequestAddressModel
            {
                Id= requestId,
                Accept = accept
            });
            var result = await _http.Post(url, form);
            var JResult = JsonConvert.DeserializeObject<AiurValue<AiurProtocol>>(result);

            if (JResult.Code != ErrorType.Success)
                throw new AiurUnexceptedResponse(JResult);
        }
    }
}
