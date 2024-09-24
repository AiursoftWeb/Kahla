using Aiursoft.AiurProtocol;
using Aiursoft.Directory.SDK.Models.ForApps.AddressModels;
using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Models.ApiViewModels;
using Aiursoft.Scanner.Abstractions;

namespace Aiursoft.Kahla.SDK.Services
{
    public class AuthService : IScopedDependency
    {
        private readonly PersistentHttpClient _tracker;
        private readonly AiurProtocolClient _http;
        private readonly KahlaLocation _kahlaLocation;

        public AuthService(
            PersistentHttpClient tracker,
            AiurProtocolClient http,
            KahlaLocation kahlaLocation)
        {
            _tracker = tracker;
            _http = http;
            _kahlaLocation = kahlaLocation;
        }

        public async Task<string> OAuthAsync()
        {
            var url = new AiurApiEndpoint(_kahlaLocation.ToString()!, "Auth", "OAuth", new { });
            var result = await _tracker.Track(url.ToString());
            return result;
        }

        public async Task<string> SignIn(int code)
        {
            var url = new AiurApiEndpoint(_kahlaLocation.ToString()!, "Auth", "AuthResult", new AuthResultAddressModel
            {
                Code = code
            });
            var result = await _tracker.Track(url.ToString());
            return result;
        }

        public async Task<AiurValue<KahlaUser>> MeAsync()
        {
            var url = new AiurApiEndpoint(_kahlaLocation.ToString()!, "Auth", "Me", new { });
            var result = await _http.Get<AiurValue<KahlaUser>>(url);
            return result;
        }

        public async Task<InitPusherViewModel> InitPusherAsync()
        {
            var url = new AiurApiEndpoint(_kahlaLocation.ToString()!, "Auth", "InitPusher", new { });
            var result = await _http.Get<InitPusherViewModel>(url);
            return result;
        }

        public async Task<AiurValue<bool>> SignInStatusAsync()
        {
            var url = new AiurApiEndpoint(_kahlaLocation.ToString()!, "Auth", "SignInStatus", new { });
            var result = await _http.Get<AiurValue<bool>>(url);
            return result;
        }

        public async Task<AiurValue<bool>> LogoffAsync()
        {
            var url = new AiurApiEndpoint(_kahlaLocation.ToString()!, "Auth", "Logoff", new { });
            var result = await _http.Get<AiurValue<bool>>(url);
            return result;
        }
    }
}
