using Aiursoft.Handler.Exceptions;
using Aiursoft.Handler.Models;
using Aiursoft.Scanner.Interfaces;
using Aiursoft.XelNaga.Models;
using Aiursoft.XelNaga.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Kahla.SDK.Services
{
    public class VersionChecker : IScopedDependency
    {
        private readonly HTTPService _http;
        private readonly IConfiguration _configuration;
        private readonly VersionService _versionService;

        public VersionChecker(
            HTTPService http,
            IConfiguration configuration,
            VersionService versionService)
        {
            _http = http;
            _configuration = configuration;
            _versionService = versionService;
        }

        public async Task<(string appVersion, string cliVersion)> CheckKahla()
        {
            var url = new AiurUrl(_configuration["KahlaMasterPackageJson"], new { });
            var response = await _http.Get(url, false);
            var result = JsonConvert.DeserializeObject<NodePackageJson>(response);

            if (result.Name.ToLower() == "kahla")
            {
                return (result.Version, _versionService.GetSDKVersion());
            }
            else
            {
                throw new AiurUnexpectedResponse(new AiurProtocol()
                {
                    Code = ErrorType.NotFound,
                    Message = "GitHub Json response is not related with Kahla!"
                });
            }
        }
    }

    public class NodePackageJson
    {
        public string Name { get; set; }
        public string Version { get; set; }
    }
}
