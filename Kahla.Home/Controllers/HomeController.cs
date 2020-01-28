using Aiursoft.Pylon.Services;
using Aiursoft.SDK.Services;
using Kahla.Home.Models.HomeViewModels;
using Kahla.SDK.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Kahla.Home.Controllers
{
    public class HomeController : Controller
    {
        private readonly VersionChecker _version;
        private readonly VersionService _versionService;
        private readonly ServiceLocation _serviceLocation;
        private readonly AiurCache _cache;

        public HomeController(
            VersionChecker version,
            VersionService versionService,
            ServiceLocation serviceLocation,
            AiurCache cache)
        {
            _version = version;
            _versionService = versionService;
            _serviceLocation = serviceLocation;
            _cache = cache;
        }

        public async Task<IActionResult> Index()
        {
            var (appVersion, cliVersion) = await _cache.GetAndCache("Version.Cache", () => _version.CheckKahla());
            var downloadSite = _serviceLocation.TryGetCDNDomain("https://download.kahla.app");
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
