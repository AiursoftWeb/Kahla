using Aiursoft.Archon.SDK.Services;
using Aiursoft.DocGenerator.Attributes;
using Aiursoft.Gateway.SDK.Models.ForApps.AddressModels;
using Aiursoft.Gateway.SDK.Services.ToGatewayServer;
using Aiursoft.Handler.Attributes;
using Aiursoft.Handler.Models;
using Aiursoft.Pylon;
using Aiursoft.Pylon.Attributes;
using Aiursoft.Pylon.Services;
using Aiursoft.Stargate.SDK.Models.ListenAddressModels;
using Aiursoft.Stargate.SDK.Services;
using Aiursoft.Stargate.SDK.Services.ToStargateServer;
using Aiursoft.Status.SDK.Models;
using Aiursoft.Status.SDK.Services.ToStatusServer;
using Aiursoft.WebTools;
using Aiursoft.XelNaga.Models;
using Kahla.SDK.Models;
using Kahla.SDK.Models.ApiAddressModels;
using Kahla.SDK.Models.ApiViewModels;
using Kahla.SDK.Services;
using Kahla.Server.Data;
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
        private readonly StargateLocator _stargateLocator;
        private readonly AuthService<KahlaUser> _authService;
        private readonly UserManager<KahlaUser> _userManager;
        private readonly SignInManager<KahlaUser> _signInManager;
        private readonly UserService _userService;
        private readonly AppsContainer _appsContainer;
        private readonly KahlaPushService _pusher;
        private readonly ChannelService _channelService;
        private readonly KahlaDbContext _dbContext;
        private readonly EventService _eventService;
        private readonly OnlineJudger _onlineJudger;
        private readonly List<DomainSettings> _appDomains;

        public AuthController(
            StargateLocator serviceLocation,
            AuthService<KahlaUser> authService,
            UserManager<KahlaUser> userManager,
            SignInManager<KahlaUser> signInManager,
            UserService userService,
            AppsContainer appsContainer,
            KahlaPushService pusher,
            ChannelService channelService,
            KahlaDbContext dbContext,
            IOptions<List<DomainSettings>> optionsAccessor,
            EventService eventService,
            OnlineJudger onlineJudger)
        {
            _stargateLocator = serviceLocation;
            _authService = authService;
            _userManager = userManager;
            _signInManager = signInManager;
            _userService = userService;
            _appsContainer = appsContainer;
            _pusher = pusher;
            _channelService = channelService;
            _dbContext = dbContext;
            _eventService = eventService;
            _onlineJudger = onlineJudger;
            _appDomains = optionsAccessor.Value;
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
            user.IsMe = true;
            try
            {
                user = await _authService.OnlyUpdate(user);
            }
            catch (WebException e)
            {
                var accessToken = await _appsContainer.AccessToken();
                await _eventService.LogAsync(accessToken, e.Message, e.StackTrace, EventLevel.Warning, HttpContext.Request.Path);
            }
            return Json(new AiurValue<KahlaUser>(user.Build(_onlineJudger))
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
            if (model.EnableInvisiable.HasValue)
            {
                currentUser.EnableInvisiable = model.EnableInvisiable == true;
            }
            if (model.MarkEmailPublic.HasValue)
            {
                currentUser.MarkEmailPublic = model.MarkEmailPublic == true;
            }
            if (model.ListInSearchResult.HasValue)
            {
                currentUser.ListInSearchResult = model.ListInSearchResult == true;
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
                ServerPath = new AiurUrl(_stargateLocator.ListenEndpoint, "Listen", "Channel", new ChannelAddressModel
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