using Aiursoft.Pylon.Exceptions;
using Aiursoft.Pylon.Interfaces;
using Aiursoft.Pylon.Models;
using Aiursoft.Pylon.Models.ForApps.AddressModels;
using Kahla.SDK.Models;
using Kahla.SDK.Models.ApiViewModels;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Kahla.SDK.Services
{
    public class AuthService : IScopedDependency
    {
        private readonly SingletonHTTP _http;
        private readonly KahlaLocation _kahlaLocation;

        public AuthService(
            SingletonHTTP http,
            KahlaLocation kahlaLocation)
        {
            _http = http;
            _kahlaLocation = kahlaLocation;
        }
        public async Task<string> OAuthAsync()
        {
            var url = new AiurUrl(_kahlaLocation.ToString(), "Auth", "OAuth", new { });
            var result = await _http.Track(url);
            return result;
        }

        public async Task<string> SignIn(int code)
        {
            var url = new AiurUrl(_kahlaLocation.ToString(), "Auth", "AuthResult", new AuthResultAddressModel
            {
                Code = code
            });
            var result = await _http.Track(url);
            return result;
        }

        public async Task<AiurValue<KahlaUser>> MeAsync()
        {
            var url = new AiurUrl(_kahlaLocation.ToString(), "Auth", "Me", new { });
            var result = await _http.Get(url);
            var JResult = JsonConvert.DeserializeObject<AiurValue<KahlaUser>>(result);

            if (JResult.Code != ErrorType.Success)
                throw new AiurUnexceptedResponse(JResult);
            return JResult;
        }

        public async Task<InitPusherViewModel> InitPusherAsync()
        {
            var url = new AiurUrl(_kahlaLocation.ToString(), "Auth", "InitPusher", new { });
            var result = await _http.Get(url);
            var JResult = JsonConvert.DeserializeObject<InitPusherViewModel>(result);

            if (JResult.Code != ErrorType.Success)
                throw new AiurUnexceptedResponse(JResult);
            return JResult;
        }

        public async Task<AiurValue<bool>> SignInStatusAsync()
        {
            var url = new AiurUrl(_kahlaLocation.ToString(), "Auth", "SignInStatus", new { });
            var result = await _http.Get(url);
            var JResult = JsonConvert.DeserializeObject<AiurValue<bool>>(result);

            if (JResult.Code != ErrorType.Success)
                throw new AiurUnexceptedResponse(JResult);
            return JResult;
        }

        public async Task<AiurValue<bool>> LogoffAsync()
        {
            var url = new AiurUrl(_kahlaLocation.ToString(), "Auth", "Logoff", new { });
            var result = await _http.Get(url);
            var JResult = JsonConvert.DeserializeObject<AiurValue<bool>>(result);

            if (JResult.Code != ErrorType.Success && JResult.Code != ErrorType.RequireAttention)
                throw new AiurUnexceptedResponse(JResult);
            return JResult;
        }
    }
}
