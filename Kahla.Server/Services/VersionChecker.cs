using Aiursoft.Pylon.Exceptions;
using Aiursoft.Pylon.Models;
using Aiursoft.Pylon.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.Server.Services
{
    public class VersionChecker
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

        public async Task<string> CheckKahla()
        {
            var url = new AiurUrl(_configuration["KahlaMasterPackageJson"], new { });
            var response = await _http.Get(url, false);
            var result = JsonConvert.DeserializeObject<NodePackageJson>(response);
            if (result.Name.ToLower() == "kahla")
            {
                return result.Version;
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
