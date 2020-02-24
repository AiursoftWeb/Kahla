using Aiursoft.Handler.Attributes;
using Aiursoft.XelNaga.Models;
using Aiursoft.XelNaga.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kahla.Home.Controllers
{
    [LimitPerMin(40)]
    [APIExpHandler]
    [APIModelStateChecker]
    public class APIController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly HTTPService _httpService;

        public APIController(
            IConfiguration configuration,
            HTTPService httpService)
        {
            _configuration = configuration;
            _httpService = httpService;
        }

        [Route("servers")]
        public async Task<IActionResult> KahlaServerList()
        {
            var serversFileAddress = _configuration["KahlaServerList"];
            var serversJson = await _httpService.Get(new AiurUrl(serversFileAddress), false);
            var servers = JsonConvert.DeserializeObject<List<string>>(serversJson);
            return Json(servers);
        }
    }
}
