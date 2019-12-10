using Aiursoft.Pylon.Services;
using Kahla.Home.Models.HomeViewModels;
using Kahla.Home.Services;
using Kahla.SDK.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Kahla.Home.Controllers
{
    public class HomeController : Controller
    {
        private readonly VersionChecker _version;
        private readonly VersionService _versionService;
        private readonly AiurCache _cache;
        private readonly ServiceLocation _serviceLocation;

        public HomeController(
            VersionChecker version,
            VersionService versionService,
            AiurCache cache,
            ServiceLocation serviceLocation)
        {
            _version = version;
            _versionService = versionService;
            _cache = cache;
            _serviceLocation = serviceLocation;
        }

        public async Task<IActionResult> Index()
        {
            var (appVersion, cliVersion) = await _cache.GetAndCache("Version.Cache", () => _version.CheckKahla());
            var downloadSite = "https://download.kahla.app";
            var model = new IndexViewModel
            {
                AppLatestVersion = appVersion,
                CLILatestVersion = cliVersion,
                SDKLatestVersion = _versionService.GetSDKVersion(),
                DownloadRoot = $"{downloadSite}/production",
                CliDownloadRoot = $"{downloadSite}/productioncli",
                ArchiveRoot = "https://github.com/AiursoftWeb/Kahla.App/archive",
                KahlaWebPath = "//web.kahla.app",
                IsProduction = true,
            };
            return View(model);
        }

        [Route("/Staging")]
        public async Task<IActionResult> Staging()
        {
            var (appVersion, cliVersion) = await _cache.GetAndCache("Version.Cache.Staging", () => _version.CheckKahlaStaging());
            var downloadSite = "https://download.kahla.app";
            var model = new IndexViewModel
            {
                AppLatestVersion = appVersion,
                CLILatestVersion = cliVersion,
                SDKLatestVersion = _versionService.GetSDKVersion(),
                DownloadRoot = $"{downloadSite}/staging",
                CliDownloadRoot = $"{downloadSite}/stagingcli",
                ArchiveRoot = "https://github.com/AiursoftWeb/Kahla.App/archive",
                KahlaWebPath = "//staging.kahla.app",
                IsProduction = false
            };
            return View(nameof(Index), model);
        }

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
