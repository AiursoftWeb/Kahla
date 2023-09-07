using Kahla.Home.Models.HomeViewModels;
using Kahla.SDK.Services;
using Microsoft.AspNetCore.Mvc;
using Aiursoft.Canon;

namespace Kahla.Home.Controllers
{
    [LimitPerMin(20)]
    public class HomeController : Controller
    {
        private readonly VersionChecker _version;
        private readonly VersionService _versionService;
        private readonly CacheService _cache;

        public HomeController(
            VersionChecker version,
            VersionService versionService,
            CacheService cache)
        {
            _version = version;
            _versionService = versionService;
            _cache = cache;
        }

        public async Task<IActionResult> Index()
        {
            var (appVersion, cliVersion) = await _cache.RunWithCache("Version.Cache", () => _version.CheckKahla());
            
            // TODO: Avoid hard code.
            var downloadSite = "https://download.kahla.app";
            var mode = Request.Host.ToString().ToLower().Contains("staging") ?
                "staging" : "production";
            var isProduction = mode == "production";
            var model = new IndexViewModel
            {
                AppLatestVersion = appVersion,
                CLILatestVersion = cliVersion,
                SDKLatestVersion = _versionService.GetSDKVersion(),
                DownloadRoot = $"{downloadSite}/{mode}",
                CliDownloadRoot = $"{downloadSite}/{mode}",
                ArchiveRoot = "https://github.com/AiursoftWeb/Kahla.App/archive",
                KahlaWebPath = isProduction ? "//web.kahla.app" : "//staging.web.kahla.app",
                IsProduction = isProduction,
            };
            return View(model);
        }

        [Route("platform-support")]
        public IActionResult PlatformSupport()
        {
            var mode = Request.Host.ToString().ToLower().Contains("staging") ?
                "staging" : "production";
            var isProduction = mode == "production";
            return View(isProduction);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
