using Aiursoft.Pylon;
using Aiursoft.Pylon.Attributes;
using Aiursoft.Pylon.Models;
using Aiursoft.Pylon.Models.ForApps.AddressModels;
using Aiursoft.Pylon.Models.Stargate.ListenAddressModels;
using Aiursoft.Pylon.Services;
using Aiursoft.Pylon.Services.ToGatewayServer;
using Aiursoft.Pylon.Services.ToStargateServer;
using Kahla.SDK.Models;
using Kahla.SDK.Models.ApiAddressModels;
using Kahla.SDK.Models.ApiViewModels;
using Kahla.SDK.Services;
using Kahla.Server.Data;
using Kahla.Server.Middlewares;
using Kahla.Server.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Kahla.Server.Controllers
{
    [LimitPerMin(40)]
    [APIExpHandler]
    [APIModelStateChecker]
    public class AuthController : Controller
    {
        private readonly ServiceLocation _serviceLocation;
        private readonly AuthService<KahlaUser> _authService;
        private readonly UserManager<KahlaUser> _userManager;
        private readonly SignInManager<KahlaUser> _signInManager;
        private readonly UserService _userService;
        private readonly AppsContainer _appsContainer;
        private readonly KahlaPushService _pusher;
        private readonly ChannelService _channelService;
        private readonly VersionChecker _version;
        private readonly VersionService _sdkVersion;
        private readonly KahlaDbContext _dbContext;
        private readonly AiurCache _cache;
        private readonly List<DomainSettings> _appDomains;

        public AuthController(
            ServiceLocation serviceLocation,
            AuthService<KahlaUser> authService,
            UserManager<KahlaUser> userManager,
            SignInManager<KahlaUser> signInManager,
            UserService userService,
            AppsContainer appsContainer,
            KahlaPushService pusher,
            ChannelService channelService,
            VersionChecker version,
            VersionService sdkVersion,
            KahlaDbContext dbContext,
            IOptions<List<DomainSettings>> optionsAccessor,
            AiurCache cache)
        {
            _serviceLocation = serviceLocation;
            _authService = authService;
            _userManager = userManager;
            _signInManager = signInManager;
            _userService = userService;
            _appsContainer = appsContainer;
            _pusher = pusher;
            _channelService = channelService;
            _version = version;
            _sdkVersion = sdkVersion;
            _dbContext = dbContext;
            _cache = cache;
            _appDomains = optionsAccessor.Value;
        }

        [APIProduces(typeof(VersionViewModel))]
        public async Task<IActionResult> Version()
        {
            var (appVersion, cliVersion) = await _cache.GetAndCache(nameof(Version), () => _version.CheckKahla());
            return Json(new VersionViewModel
            {
                LatestVersion = appVersion,
                LatestCLIVersion = cliVersion,
                APIVersion = _sdkVersion.GetSDKVersion(),
                Message = "Successfully get the latest version number for Kahla App and Kahla.CLI.",
                DownloadAddress = "https://www.kahla.app"
            });
        }

        [AiurForceAuth("", "", justTry: false, register: false)]
        public IActionResult OAuth()
        {
            return this.Protocol(ErrorType.RequireAttention, "You are already signed in. But you are still trying to call OAuth action. Just use Kahla directly!");
        }

        [AiurForceAuth("", "", justTry: false, register: true)]
        public IActionResult GoRegister()
        {
            return this.Protocol(ErrorType.RequireAttention, "You are already signed in. But you are still trying to call OAuth action. Just use Kahla directly!");
        }

        public async Task<IActionResult> AuthResult(AuthResultAddressModel model)
        {
            var user = await _authService.AuthApp(model, isPersistent: true);
            this.SetClientLang(user.PreferedLanguage);
            var domain = _appDomains.FirstOrDefault(t => t.Server.ToLower().Trim() == Request.Host.ToString().ToLower().Trim());
            if (domain == null)
            {
                return NotFound();
            }
            if (!await _dbContext.AreFriends(user.Id, user.Id))
            {
                _dbContext.AddFriend(user.Id, user.Id);
                await _dbContext.SaveChangesAsync();
            }
            return Redirect(domain.Client);
        }

        [APIProduces(typeof(AiurValue<bool>))]
        public async Task<IActionResult> SignInStatus()
        {
            var user = await GetKahlaUser();
            var signedIn = user != null;
            return Json(new AiurValue<bool>(signedIn)
            {
                Code = ErrorType.Success,
                Message = "Successfully get your signin status."
            });
        }

        [AiurForceAuth(directlyReject: true)]
        [APIProduces(typeof(AiurValue<KahlaUser>))]
        public async Task<IActionResult> Me()
        {
            var user = await GetKahlaUser();
            try
            {
                user = await _authService.OnlyUpdate(user);
            }
            catch (WebException) { }
            user.IsMe = true;
            return Json(new AiurValue<KahlaUser>(user)
            {
                Code = ErrorType.Success,
                Message = "Successfully get your information."
            });
        }

        [HttpPost]
        [AiurForceAuth(directlyReject: true)]
        public async Task<IActionResult> UpdateInfo(UpdateInfoAddressModel model)
        {
            var currentUser = await GetKahlaUser();
            currentUser.IconFilePath = model.HeadIconPath;
            currentUser.NickName = model.NickName;
            currentUser.Bio = model.Bio;
            currentUser.MakeEmailPublic = !model.HideMyEmail;
            await _userService.ChangeProfileAsync(currentUser.Id, await _appsContainer.AccessToken(), currentUser.NickName, model.HeadIconPath, currentUser.Bio);
            await _userManager.UpdateAsync(currentUser);
            return this.Protocol(ErrorType.Success, "Successfully set your personal info.");
        }

        [HttpPost]
        [AiurForceAuth(true)]
        public async Task<IActionResult> UpdateClientSetting(UpdateClientSettingAddressModel model)
        {
            var currentUser = await GetKahlaUser();
            if (model.ThemeId.HasValue)
            {
                currentUser.ThemeId = model.ThemeId ?? 0;
            }
            if (model.EnableEmailNotification.HasValue)
            {
                currentUser.EnableEmailNotification = model.EnableEmailNotification == true;
            }
            if (model.EnableEnterToSendMessage.HasValue)
            {
                currentUser.EnableEnterToSendMessage = model.EnableEnterToSendMessage == true;
            }
            await _userManager.UpdateAsync(currentUser);
            return this.Protocol(ErrorType.Success, "Successfully update your client setting.");
        }

        [HttpPost]
        [AiurForceAuth(directlyReject: true)]
        public async Task<IActionResult> ChangePassword(ChangePasswordAddressModel model)
        {
            var currentUser = await GetKahlaUser();
            await _userService.ChangePasswordAsync(currentUser.Id, await _appsContainer.AccessToken(), model.OldPassword, model.NewPassword);
            return this.Protocol(ErrorType.Success, "Successfully changed your password!");
        }

        [HttpPost]
        [AiurForceAuth(directlyReject: true)]
        public async Task<IActionResult> SendEmail([EmailAddress][Required] string email)
        {
            var user = await GetKahlaUser();
            var token = await _appsContainer.AccessToken();
            var result = await _userService.SendConfirmationEmailAsync(token, user.Id, email);
            return Json(result);
        }

        [AiurForceAuth(directlyReject: true)]
        [APIProduces(typeof(InitPusherViewModel))]
        public async Task<IActionResult> InitPusher()
        {
            var user = await GetKahlaUser();
            if (user.CurrentChannel == -1 || (await _channelService.ValidateChannelAsync(user.CurrentChannel, user.ConnectKey)).Code != ErrorType.Success)
            {
                var channel = await _pusher.ReCreateStargateChannel(user.Id);
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
            return Json(model);
        }

        public async Task<IActionResult> LogOff(LogOffAddressModel model)
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await GetKahlaUser();
                var device = await _dbContext
                    .Devices
                    .Where(t => t.UserId == user.Id)
                    .SingleOrDefaultAsync(t => t.Id == model.DeviceId);
                await _signInManager.SignOutAsync();
                if (device == null)
                {
                    return this.Protocol(ErrorType.RequireAttention, "Successfully logged you off, but we did not find device with id: " + model.DeviceId);
                }
                else
                {
                    _dbContext.Devices.Remove(device);
                    await _dbContext.SaveChangesAsync();
                    return this.Protocol(ErrorType.Success, "Success.");
                }
            }
            else
            {
                return this.Protocol(ErrorType.RequireAttention, "You are not authorized at all. But you can still call this API.");
            }
        }

        private Task<KahlaUser> GetKahlaUser() => _userManager.GetUserAsync(User);
    }
}