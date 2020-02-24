using Aiursoft.Handler.Attributes;
using Aiursoft.Pylon.Services;
using Aiursoft.XelNaga.Models;
using Aiursoft.XelNaga.Services;
using Kahla.SDK.Models.ApiViewModels;
using Kahla.SDK.Services;
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
        private readonly HomeService _homeService;
        private readonly HTTPService _httpService;
        private readonly AiurCache _cache;

        public APIController(
            IConfiguration configuration,
            HomeService homeService,
            HTTPService httpService,
            AiurCache cache)
        {
            _configuration = configuration;
            _homeService = homeService;
            _httpService = httpService;
            _cache = cache;
        }

        [Route("servers")]
        public async Task<IActionResult> KahlaServerList()
        {
            var serversFileAddress = _configuration["KahlaServerList"];
            var serversJson = await _cache.GetAndCache($"servers-list", () => _httpService.Get(new AiurUrl(serversFileAddress), false));
            var servers = JsonConvert.DeserializeObject<List<string>>(serversJson);
            var serversRendered = new List<IndexViewModel>();
            var taskList = new List<Task>();
            foreach (var server in servers)
            {
                async Task AddServer()
                {
                    try
                    {
                        var serverInfo = await _cache.GetAndCache($"server-detail-{server}", () => _homeService.IndexAsync(server));
                        if (serverInfo != null)
                        {
                            serversRendered.Add(serverInfo);
                        }
                    }
                    catch
                    {
                        _cache.GetAndCache($"server-detail-{server}", () => (IndexViewModel)null);
                    }
                }
                taskList.Add(AddServer());
            }
            await Task.WhenAll(taskList);
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            return Json(serversRendered);
        }
    }
}
