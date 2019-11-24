using Aiursoft.Pylon.Exceptions;
using Aiursoft.Pylon.Interfaces;
using Aiursoft.Pylon.Models;
using Aiursoft.Pylon.Services;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Kahla.SDK.Services
{
    public class HomeService : IScopedDependency
    {
        private readonly HTTPService _http;
        private readonly KahlaLocation _kahlaLocation;

        public HomeService(
            HTTPService http,
            KahlaLocation kahlaLocation)
        {
            _http = http;
            _kahlaLocation = kahlaLocation;
        }

        public async Task<AiurValue<DateTime>> IndexAsync()
        {
            var url = new AiurUrl(_kahlaLocation.ToString(), "Home", "Index", new { });
            var result = await _http.Get(url, true);
            var JResult = JsonConvert.DeserializeObject<AiurValue<DateTime>>(result);

            if (JResult.Code != ErrorType.Success)
                throw new AiurUnexceptedResponse(JResult);
            return JResult;
        }
    }
}
