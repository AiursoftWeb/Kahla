using Aiursoft.Pylon;
using Aiursoft.Pylon.Attributes;
using Aiursoft.Pylon.Exceptions;
using Aiursoft.Pylon.Models;
using Aiursoft.Pylon.Models.ForApps.AddressModels;
using Aiursoft.Pylon.Models.Stargate.ListenAddressModels;
using Aiursoft.Pylon.Services;
using Aiursoft.Pylon.Services.ToAPIServer;
using Aiursoft.Pylon.Services.ToStargateServer;
using Kahla.Server.Models;
using Kahla.Server.Models.ApiAddressModels;
using Kahla.Server.Models.ApiViewModels;
using Kahla.Server.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
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
        private readonly UserManager<KahlaUser> _userManager;
        private readonly SignInManager<KahlaUser> _signInManager;
        private readonly UserService _userService;
        private readonly AppsContainer _appsContainer;
        private readonly PushKahlaMessageService _pusher;
        private readonly ChannelService _channelService;

        public AuthController(
            ServiceLocation serviceLocation,
            IConfiguration configuration,
            IHostingEnvironment env,
            AuthService<KahlaUser> authService,
            OAuthService oauthService,
            UserManager<KahlaUser> userManager,
            SignInManager<KahlaUser> signInManager,
            UserService userService,
            AppsContainer appsContainer,
            PushKahlaMessageService pusher,
            ChannelService channelService)
        {
            _serviceLocation = serviceLocation;
            _configuration = configuration;
            _env = env;
            _authService = authService;
            _oauthService = oauthService;
            _userManager = userManager;
            _signInManager = signInManager;
            _userService = userService;
            _appsContainer = appsContainer;
            _pusher = pusher;
            _channelService = channelService;
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
            return this.AiurJson(new AiurProtocal()
            {
                Code = ErrorType.Success,
                Message = "Auth success."
            });
        }


        [HttpPost]
        public async Task<IActionResult> RegisterKahla(RegisterKahlaAddressModel model)
        {
            var result = await _oauthService.AppRegisterAsync(model.Email, model.Password, model.ConfirmPassword);
            return this.AiurJson(result);
        }

        public async Task<IActionResult> SignInStatus()
        {
            var user = await GetKahlaUser();
            var signedIn = user != null;
            return this.AiurJson(new AiurValue<bool>(signedIn)
            {
                Code = ErrorType.Success,
                Message = "Successfully get your signin status."
            });
        }

        [AiurForceAuth(directlyReject: true)]
        public async Task<IActionResult> Me()
        {
            var user = await GetKahlaUser();
            return this.AiurJson(new AiurValue<KahlaUser>(user)
            {
                Code = ErrorType.Success,
                Message = "Successfully get your information."
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateInfo(UpdateInfoAddressModel model)
        {
            var cuser = await GetKahlaUser();
            cuser.HeadImgFileKey = model.HeadImgKey;
            cuser.NickName = model.NickName;
            cuser.Bio = model.Bio;
            await _userService.ChangeProfileAsync(cuser.Id, await _appsContainer.AccessToken(), cuser.NickName, cuser.HeadImgFileKey, cuser.Bio);
            await _userManager.UpdateAsync(cuser);
            return this.Protocal(ErrorType.Success, "Successfully set your personal info.");
        }

        [HttpPost]
        [AiurForceAuth(directlyReject: true)]
        public async Task<IActionResult> ChangePassword(ChangePasswordAddresModel model)
        {
            var cuser = await GetKahlaUser();
            var result = await _userService.ChangePasswordAsync(cuser.Id, await _appsContainer.AccessToken(), model.OldPassword, model.NewPassword);
            return this.Protocal(ErrorType.Success, "Successfully changed your password!");
        }


        [AiurForceAuth(directlyReject: true)]
        public async Task<IActionResult> InitPusher()
        {
            var user = await GetKahlaUser();
            if (user.CurrentChannel == -1 || (await _channelService.ValidateChannelAsync(user.CurrentChannel, user.ConnectKey)).Code != ErrorType.Success)
            {
                var channel = await _pusher.Init(user.Id);
                user.CurrentChannel = channel.ChannelId;
                user.ConnectKey = channel.ConnectKey;
                await _userManager.UpdateAsync(user);
            }
            var model = new InitPusherViewModel
            {
                Code = ErrorType.Success,
                Message = "Successfully get your channel.",
                ChannelId = user.CurrentChannel,
                ConnectKey = user.ConnectKey,
                ServerPath = new AiurUrl(_serviceLocation.StargateListenAddress, "Listen", "Channel", new ChannelAddressModel
                {
                    Id = user.CurrentChannel,
                    Key = user.ConnectKey
                }).ToString()
            };
            return this.AiurJson(model);
        }

        public async Task<IActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();
            return this.Protocal(ErrorType.Success, "Success.");
        }

        private async Task<KahlaUser> GetKahlaUser()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return null;
            }
            return await _userManager.FindByNameAsync(User.Identity.Name);
        }
    }
}
