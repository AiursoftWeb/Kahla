using Aiursoft.Pylon.Exceptions;
using Aiursoft.Pylon.Interfaces;
using Aiursoft.Pylon.Models;
using Aiursoft.Pylon.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Kahla.Server.Services
{
    public class VersionChecker : IScopedDependency
    {
        private readonly HTTPService _http;
        private readonly IConfiguration _configuration;
        public VersionChecker(
            HTTPService http,
            IConfiguration configuration
            )
        {
            _http = http;
            _configuration = configuration;
        }

        public async Task<(string appVersion, string cliVersion)> CheckKahla()
        {
            var url = new AiurUrl(_configuration["KahlaMasterPackageJson"], new { });
            var response = await _http.Get(url, false);
            var result = JsonConvert.DeserializeObject<NodePackageJson>(response);

            var urlcli = new AiurUrl(_configuration["CLIMasterPackageJson"], new { });
            var responsecli = await _http.Get(urlcli, false);
            var resultcli = JsonConvert.DeserializeObject<NodePackageJson>(responsecli);

            if (result.Name.ToLower() == "kahla")
            {
                return (result.Version, resultcli.Version);
            }
            else
            {
                throw new AiurUnexceptedResponse(new AiurProtocol()
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
