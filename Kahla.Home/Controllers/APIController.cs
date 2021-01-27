using Aiursoft.Handler.Attributes;
using Aiursoft.XelNaga.Models;
using Aiursoft.XelNaga.Services;
using Kahla.SDK.Models.ApiViewModels;
using Kahla.SDK.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Concurrent;
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
        private readonly HttpService _httpService;
        private readonly AiurCache _cache;
        private readonly VersionChecker _version;

        public APIController(
            IConfiguration configuration,
            HomeService homeService,
            HttpService httpService,
            AiurCache cache,
            VersionChecker version)
        {
            _configuration = configuration;
            _homeService = homeService;
            _httpService = httpService;
            _cache = cache;
            _version = version;
        }

        [Route("servers")]
        public async Task<IActionResult> KahlaServerList()
        {
            var serversFileAddress = _configuration["KahlaServerList"];
            var serversJson = await _cache.GetAndCache("servers-list", () => _httpService.Get(new AiurUrl(serversFileAddress)));
            var servers = JsonConvert.DeserializeObject<List<string>>(serversJson);
            var serversRendered = new ConcurrentBag<IndexViewModel>();
            await servers.ForEachInThreadsPool(async server =>
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
                    // ignored
                }
            });
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            return Json(serversRendered);
        }

        [Route("version")]
        public async Task<IActionResult> Version()
        {
            var (appVersion, cliVersion) = await _cache.GetAndCache(nameof(Version), () => _version.CheckKahla());
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            return Json(new VersionViewModel
            {
                LatestVersion = appVersion,
                LatestCLIVersion = cliVersion,
                Message = "Successfully get the latest version number for Kahla App and Kahla.CLI.",
                DownloadAddress = $"{Request.Scheme}://{Request.Host}"
            });
        }
    }
}
