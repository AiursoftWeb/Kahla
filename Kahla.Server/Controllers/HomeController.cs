using Aiursoft.Pylon.Attributes;
using Aiursoft.Pylon.Models;
using Aiursoft.Pylon.Services;
using Kahla.SDK.Attributes;
using Kahla.SDK.Models;
using Kahla.SDK.Models.ApiViewModels;
using Kahla.SDK.Services;
using Kahla.Server.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Kahla.Server.Controllers
{
    [LimitPerMin(40)]
    [OnlineDetector]
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly ServiceLocation _serviceLocation;
        private readonly KahlaDbContext _dbContext;
        private readonly AuthService<KahlaUser> _authService;
        private readonly AppsContainer _appsContainer;
        private readonly VersionService _sdkVersion;

        public HomeController(
            IWebHostEnvironment env,
            ServiceLocation serviceLocation,
            KahlaDbContext dbContext,
            AuthService<KahlaUser> authService,
            AppsContainer appsContainer,
            VersionService sdkVersion)
        {
            _env = env;
            _serviceLocation = serviceLocation;
            _dbContext = dbContext;
            _authService = authService;
            _appsContainer = appsContainer;
            _sdkVersion = sdkVersion;
        }

        [APIProduces(typeof(IndexViewModel))]
        public IActionResult Index()
        {
            return Json(new IndexViewModel
            {
                Code = ErrorType.Success,
                Message = $"Welcome to Aiursoft Kahla API! Running in {_env.EnvironmentName} mode.",
                WikiPath = _serviceLocation.Wiki,
                ServerTime = DateTime.Now,
                UTCTime = DateTime.UtcNow,
                APIVersion = _sdkVersion.GetSDKVersion()
            });
        }

        public async Task<IActionResult> Upgrade()
        {
            var users = await _dbContext.Users.ToListAsync();
            await _appsContainer.AccessToken();
            foreach (var user in users)
            {
                try
                {
                    await _authService.OnlyUpdate(user);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
            }
            return Json("");
        }
    }
}
