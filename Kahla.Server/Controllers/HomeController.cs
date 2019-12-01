using Aiursoft.Pylon.Attributes;
using Aiursoft.Pylon.Models;
using Aiursoft.Pylon.Services;
using Kahla.SDK.Models;
using Kahla.Server.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Kahla.Server.Controllers
{
    [LimitPerMin(40)]
    public class HomeController : Controller
    {
        private readonly ServiceLocation _serviceLocation;
        private readonly KahlaDbContext _dbContext;
        private readonly AuthService<KahlaUser> _authService;
        private readonly AppsContainer _appsContainer;

        public HomeController(
            ServiceLocation serviceLocation,
            KahlaDbContext dbContext,
            AuthService<KahlaUser> authService,
            AppsContainer appsContainer)
        {
            _serviceLocation = serviceLocation;
            _dbContext = dbContext;
            _authService = authService;
            _appsContainer = appsContainer;
        }

        [APIProduces(typeof(AiurValue<DateTime>))]
        public IActionResult Index()
        {
            return Json(new AiurValue<DateTime>(DateTime.UtcNow)
            {
                Code = ErrorType.Success,
                Message = "Welcome to Aiursoft Kahla server! View our wiki at: " + _serviceLocation.Wiki
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
