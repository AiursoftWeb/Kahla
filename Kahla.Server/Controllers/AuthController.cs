using Aiursoft.Pylon;
using Aiursoft.Pylon.Attributes;
using Aiursoft.Pylon.Exceptions;
using Aiursoft.Pylon.Models;
using Aiursoft.Pylon.Models.ForApps.AddressModels;
using Aiursoft.Pylon.Models.Stargate.ListenAddressModels;
using Aiursoft.Pylon.Services;
using Aiursoft.Pylon.Services.ToAPIServer;
using Aiursoft.Pylon.Services.ToStargateServer;
using Kahla.Server.Data;
using Kahla.Server.Models;
using Kahla.Server.Models.ApiAddressModels;
using Kahla.Server.Models.ApiViewModels;
using Kahla.Server.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.Server.Controllers
{
    [APIExpHandler]
    [APIModelStateChecker]
    public class AuthController : Controller
    {
        private readonly ServiceLocation _serviceLocation;
        private readonly IHostingEnvironment _env;
        private readonly AuthService<KahlaUser> _authService;
        private readonly OAuthService _oauthService;
        private readonly UserManager<KahlaUser> _userManager;
        private readonly SignInManager<KahlaUser> _signInManager;
        private readonly UserService _userService;
        private readonly AppsContainer _appsContainer;
        private readonly KahlaPushService _pusher;
        private readonly ChannelService _channelService;
        private readonly VersionChecker _version;
        private readonly KahlaDbContext _dbContext;

        public AuthController(
            ServiceLocation serviceLocation,
            IHostingEnvironment env,
            AuthService<KahlaUser> authService,
            OAuthService oauthService,
            UserManager<KahlaUser> userManager,
            SignInManager<KahlaUser> signInManager,
            UserService userService,
            AppsContainer appsContainer,
            KahlaPushService pusher,
            ChannelService channelService,
            VersionChecker version,
            KahlaDbContext dbContext)
        {
            _serviceLocation = serviceLocation;
            _env = env;
            _authService = authService;
            _oauthService = oauthService;
            _userManager = userManager;
            _signInManager = signInManager;
            _userService = userService;
            _appsContainer = appsContainer;
            _pusher = pusher;
            _channelService = channelService;
            _version = version;
            _dbContext = dbContext;
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

        public async Task<IActionResult> Version()
        {
            var latest = await _version.CheckKahla();
            return this.AiurJson(new VersionViewModel
            {
                LatestVersion = latest,
                OldestSupportedVersion = latest,
                Message = "Successfully get the lastest version number for Kahla App.",
                DownloadAddress = "https://www.kahla.app"
            });
        }

        [HttpPost]
        public async Task<IActionResult> AuthByPassword(AuthByPasswordAddressModel model)
        {
            var pack = await _oauthService.PasswordAuthAsync(Extends.CurrentAppId, model.Email, model.Password);
            if (pack.Code != ErrorType.Success)
            {
                return this.Protocol(ErrorType.Unauthorized, pack.Message);
            }
            var user = await _authService.AuthApp(new AuthResultAddressModel
            {
                code = pack.Value,
                state = string.Empty
            }, isPersistent: true);
            if (!await _dbContext.AreFriends(user.Id, user.Id))
            {
                _dbContext.AddFriend(user.Id, user.Id);
                await _dbContext.SaveChangesAsync();
            }
            return this.AiurJson(new AiurProtocol()
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
            user = await _authService.OnlyUpdate(user);
            user.IsMe = true;
            return this.AiurJson(new AiurValue<KahlaUser>(user)
            {
                Code = ErrorType.Success,
                Message = "Successfully get your information."
            });
        }

        [HttpPost]
        [AiurForceAuth(directlyReject: true)]
        public async Task<IActionResult> UpdateInfo(UpdateInfoAddressModel model)
        {
            var cuser = await GetKahlaUser();
            cuser.HeadImgFileKey = model.HeadImgKey;
            cuser.NickName = model.NickName;
            cuser.Bio = model.Bio;
            cuser.MakeEmailPublic = !model.HideMyEmail;
            await _userService.ChangeProfileAsync(cuser.Id, await _appsContainer.AccessToken(), cuser.NickName, cuser.HeadImgFileKey, cuser.Bio);
            await _userManager.UpdateAsync(cuser);
            return this.Protocol(ErrorType.Success, "Successfully set your personal info.");
        }

        [HttpPost]
        [AiurForceAuth(directlyReject: true)]
        public async Task<IActionResult> ChangePassword(ChangePasswordAddresModel model)
        {
            var cuser = await GetKahlaUser();
            var result = await _userService.ChangePasswordAsync(cuser.Id, await _appsContainer.AccessToken(), model.OldPassword, model.NewPassword);
            return this.Protocol(ErrorType.Success, "Successfully changed your password!");
        }

        [HttpPost]
        [AiurForceAuth(directlyReject: true)]
        public async Task<IActionResult> SendEmail([EmailAddress][Required]string email)
        {
            var user = await GetKahlaUser();
            var token = await _appsContainer.AccessToken();
            var result = await _userService.SendConfirmationEmailAsync(token, user.Id, email);
            return Json(result);
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

        [AiurForceAuth(directlyReject: true)]
        public async Task<IActionResult> LogOff(LogOffAddressModel model)
        {
            var user = await GetKahlaUser();
            var device = await _dbContext
                .Devices
                .Where(t => t.UserID == user.Id)
                .SingleOrDefaultAsync(t => t.Id == model.DeviceId);
            await _signInManager.SignOutAsync();
            if (device == null)
            {
                return this.Protocol(ErrorType.RequireAttention, "Successfully logged you off, but we did not find deivce with id: " + model.DeviceId);
            }
            _dbContext.Devices.Remove(device);
            await _dbContext.SaveChangesAsync();
            return this.Protocol(ErrorType.Success, "Success.");
        }

        private Task<KahlaUser> GetKahlaUser()
        {
            return _userManager.GetUserAsync(User);
        }

        [HttpPost]
        [AiurForceAuth(directlyReject: true)]
        public async Task<IActionResult> AddDevice(AddDeviceAddressModel model)
        {
            var user = await GetKahlaUser();
            if (_dbContext.Devices.Any(t => t.PushP256DH == model.PushP256DH))
            {
                return this.Protocol(ErrorType.HasDoneAlready, "There is already an device with push 256DH: " + model.PushP256DH);
            }
            var device = new Device
            {
                Name = model.Name,
                UserID = user.Id,
                PushAuth = model.PushAuth,
                PushEndpoint = model.PushEndpoint,
                PushP256DH = model.PushP256DH
            };
            _dbContext.Devices.Add(device);
            await _dbContext.SaveChangesAsync();
            //ErrorType.Success, 
            return this.AiurJson(new AiurValue<long>(device.Id)
            {
                Code = ErrorType.Success,
                Message = "Successfully created your new device with id: " + device.Id
            });
        }

        [HttpPost]
        [AiurForceAuth(directlyReject: true)]
        public async Task<IActionResult> UpdateDevice(UpdateDeviceAddressModel model)
        {
            var user = await GetKahlaUser();
            var device = await _dbContext
                .Devices
                .Where(t => t.UserID == user.Id)
                .SingleOrDefaultAsync(t => t.Id == model.DeviceId);
            if (device == null)
            {
                return this.Protocol(ErrorType.NotFound, "Can not find a deivce with ID: " + model.DeviceId);
            }
            device.Name = model.Name;
            device.PushAuth = model.PushAuth;
            device.PushEndpoint = model.PushEndpoint;
            device.PushP256DH = model.PushP256DH;
            _dbContext.Devices.Update(device);
            await _dbContext.SaveChangesAsync();
            //ErrorType.Success, 
            return this.AiurJson(new AiurValue<Device>(device)
            {
                Code = ErrorType.Success,
                Message = "Successfully updated your new device with id: " + device.Id
            });
        }

        [AiurForceAuth(directlyReject: true)]
        public async Task<IActionResult> MyDevices()
        {
            var user = await GetKahlaUser();
            var devices = await _dbContext
                .Devices
                .Where(t => t.UserID == user.Id)
                .ToListAsync();
            return this.AiurJson(new AiurCollection<Device>(devices)
            {
                Code = ErrorType.Success,
                Message = "Successfully get all your devices."
            });
        }
    }
}
