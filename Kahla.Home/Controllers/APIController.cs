using Aiursoft.Handler.Attributes;
using Aiursoft.WebTools;
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
using Aiursoft.Canon;

namespace Kahla.Home.Controllers
{
    [LimitPerMin(40)]
    [APIRemoteExceptionHandler]
    [APIModelStateChecker]
    public class APIController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly HomeService _homeService;
        private readonly HttpService _httpService;
        private readonly CacheService _cache;
        private readonly CanonPool _pool;
        private readonly VersionChecker _version;

        public APIController(
            IConfiguration configuration,
            HomeService homeService,
            HttpService httpService,
            CacheService cache,
            CanonPool pool,
            VersionChecker version)
        {
            _configuration = configuration;
            _homeService = homeService;
            _httpService = httpService;
            _cache = cache;
            _pool = pool;
            _version = version;
        }

        [Route("servers")]
        public async Task<IActionResult> KahlaServerList()
        {
            var serversFileAddress = _configuration["KahlaServerList"];
            var serversJson = await _cache.RunWithCache("servers-list", () => _httpService.Get(new AiurUrl(serversFileAddress)));
            var servers = JsonConvert.DeserializeObject<List<string>>(serversJson);
            var serversRendered = new ConcurrentBag<IndexViewModel>();
            foreach (var server in servers)
            {
                _pool.RegisterNewTaskToPool(async () =>
                {
                    var serverInfo = await _cache.RunWithCache($"server-detail-{server}", () => _homeService.IndexAsync(server));
                    if (serverInfo != null)
                    {
                        serversRendered.Add(serverInfo);
                    }
                });
            }
            await _pool.RunAllTasksInPoolAsync();
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            return Ok(serversRendered);
        }

        [Route("version")]
        public async Task<IActionResult> Version()
        {
            var (appVersion, cliVersion) = await _cache.RunWithCache(nameof(Version), () => _version.CheckKahla());
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            return this.Protocol(new VersionViewModel
            {
                LatestVersion = appVersion,
                LatestCLIVersion = cliVersion,
                Message = "Successfully get the latest version number for Kahla App and Kahla.CLI.",
                DownloadAddress = $"{Request.Scheme}://{Request.Host}"
            });
        }
    }
}
