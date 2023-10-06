using Kahla.Home.Models.HomeViewModels;
using Kahla.SDK.Services;
using Microsoft.AspNetCore.Mvc;
using Aiursoft.Canon;

namespace Kahla.Home.Controllers
{
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
            
            var downloadSite = "https://download.kahla.app";
            var model = new IndexViewModel
            {
                AppLatestVersion = appVersion,
                CLILatestVersion = cliVersion,
                SDKLatestVersion = _versionService.GetSDKVersion(),
                DownloadRoot = $"{downloadSite}/production",
                CliDownloadRoot = $"{downloadSite}/production",

                // TODO: Avoid hard code.
                KahlaWebPath = "https://web.kahla.app"
            };
            return View(model);
        }

        [Route("platform-support")]
        public IActionResult PlatformSupport()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
