using Aiursoft.Pylon;
using Aiursoft.Pylon.Attributes;
using Aiursoft.Pylon.Exceptions;
using Aiursoft.Pylon.Models;
using Aiursoft.Pylon.Models.ForApps.AddressModels;
using Aiursoft.Pylon.Services;
using Aiursoft.Pylon.Services.ToAPIServer;
using Kahla.Server.Models;
using Kahla.Server.Models.ApiAddressModels;
using Kahla.Server.Models.ApiViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.Server.Controllers
{
    [APIExpHandler]
    [APIModelStateChecker]
    public class AuthController : Controller
    {
        private readonly ServiceLocation _serviceLocation;
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _env;
        private readonly AuthService<KahlaUser> _authService;
        private readonly OAuthService _oauthService;

        public AuthController(
            ServiceLocation serviceLocation,
            IConfiguration configuration,
            IHostingEnvironment env,
            AuthService<KahlaUser> authService,
            OAuthService oauthService)
        {
            _serviceLocation = serviceLocation;
            _configuration = configuration;
            _env = env;
            _authService = authService;
            _oauthService = oauthService;
        }

        public IActionResult Index()
        {
            return this.AiurJson(new
            {
                Code = ErrorType.Success,
                Message = $"Welcome to Aiursoft Kahla API! Running in {_env.EnvironmentName} mode.",
                WikiPath = _serviceLocation.Wiki,
                ServerTime = DateTime.Now,
                UTCTime = DateTime.UtcNow
            });
        }

        public IActionResult Version()
        {
            return this.AiurJson(new VersionViewModel
            {
                LatestVersion = _configuration["AppVersion"],
                OldestSupportedVersion = _configuration["AppVersion"],
                Message = "Successfully get the lastest version number for Kahla App.",
                DownloadAddress = _serviceLocation.KahlaHome
            });
        }

        [HttpPost]
        public async Task<IActionResult> AuthByPassword(AuthByPasswordAddressModel model)
        {
            try
            {
                var pack = await _oauthService.PasswordAuthAsync(Extends.CurrentAppId, model.Email, model.Password);
                if (pack.Code != ErrorType.Success)
                {
                    return this.Protocal(ErrorType.Unauthorized, pack.Message);
                }
                var user = await _authService.AuthApp(new AuthResultAddressModel
                {
                    code = pack.Value,
                    state = string.Empty
                }, isPersistent: true);
            }
            catch (AiurUnexceptedResponse e)
            {
                return this.AiurJson(e.Response);
            }
            return this.AiurJson(new AiurProtocal()
            {
                Code = ErrorType.Success,
                Message = "Auth success."
            });
        }
    }
}
