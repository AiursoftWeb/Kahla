using Aiursoft.Handler.Exceptions;
using Aiursoft.Handler.Models;
using Aiursoft.Scanner.Interfaces;
using Aiursoft.XelNaga.Models;
using Aiursoft.XelNaga.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Kahla.Home.Services
{
    public class VersionChecker : ITransientDependency
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
        public async Task<(string appVersion, string cliVersion)> CheckKahlaStaging()
        {
            var url = new AiurUrl(_configuration["KahlaDevPackageJson"], new { });
            var response = await _http.Get(url, false);
            var result = JsonConvert.DeserializeObject<NodePackageJson>(response);

            var urlcli = new AiurUrl(_configuration["CLIDevPackageJson"], new { });
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
