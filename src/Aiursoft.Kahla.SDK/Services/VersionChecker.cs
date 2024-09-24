using Aiursoft.AiurProtocol;
using Aiursoft.Scanner.Abstractions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Aiursoft.Kahla.SDK.Services
{
    public class VersionChecker : IScopedDependency
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _configuration;
        private readonly VersionService _versionService;

        public VersionChecker(
            HttpClient http,
            IConfiguration configuration,
            VersionService versionService)
        {
            _http = http;
            _configuration = configuration;
            _versionService = versionService;
        }

        public async Task<(string appVersion, string cliVersion)> CheckKahla()
        {
            var response = await _http.GetStringAsync(_configuration["KahlaMasterPackageJson"]);
            var result = JsonConvert.DeserializeObject<NodePackageJson>(response);
            if (result.Name.ToLower() == "kahla")
            {
                return (result.Version, _versionService.GetSDKVersion());
            }
            else
            {
                throw new AiurServerException(Code.Conflict, "GitHub Json response is not related with Kahla!");
            }
        }
    }

    public class NodePackageJson
    {
        public string Name { get; set; }
        public string Version { get; set; }
    }
}
