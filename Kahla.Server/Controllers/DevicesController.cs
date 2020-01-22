using Aiursoft.DocGenerator.Attributes;
using Aiursoft.Handler.Attributes;
using Aiursoft.Handler.Models;
using Aiursoft.Pylon;
using Aiursoft.Pylon.Attributes;
using Aiursoft.SDK.Services;
using Aiursoft.SDK.Services.ToStargateServer;
using Kahla.SDK.Events;
using Kahla.SDK.Models;
using Kahla.SDK.Models.ApiAddressModels;
using Kahla.Server.Data;
using Kahla.Server.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.Server.Controllers
{
    [LimitPerMin(40)]
    [APIExpHandler]
    [APIModelStateChecker]
    [AiurForceAuth(directlyReject: true)]
    public class DevicesController : Controller
    {
        private readonly KahlaDbContext _dbContext;
        private readonly ThirdPartyPushService _thirdPartyPushService;
        private readonly UserManager<KahlaUser> _userManager;
        private readonly PushMessageService _stargatePushService;
        private readonly AppsContainer _appsContainer;

        public DevicesController(
            KahlaDbContext dbContext,
            ThirdPartyPushService thirdPartyPushService,
            UserManager<KahlaUser> userManager,
            PushMessageService stargatePushService,
            AppsContainer appsContainer)
        {
            _dbContext = dbContext;
            _thirdPartyPushService = thirdPartyPushService;
            _userManager = userManager;
            _stargatePushService = stargatePushService;
            _appsContainer = appsContainer;
        }

        [HttpPost]
        [APIProduces(typeof(AiurValue<int>))]
        public async Task<IActionResult> AddDevice(AddDeviceAddressModel model)
        {
            var user = await GetKahlaUser();
            if (_dbContext.Devices.Any(t => t.PushP256DH == model.PushP256DH))
            {
                return this.Protocol(ErrorType.HasDoneAlready, "There is already an device with push 256DH: " + model.PushP256DH);
            }
            var devicesExists = await _dbContext.Devices.Where(t => t.UserId == user.Id).ToListAsync();
            if (devicesExists.Count >= 10)
            {
                var toDrop = devicesExists.OrderBy(t => t.AddTime).First();
                _dbContext.Devices.Remove(toDrop);
                await _dbContext.SaveChangesAsync();
            }
            var device = new Device
            {
                Name = model.Name,
                UserId = user.Id,
                PushAuth = model.PushAuth,
                PushEndpoint = model.PushEndpoint,
                PushP256DH = model.PushP256DH,
                IPAddress = HttpContext.Connection.RemoteIpAddress.ToString()
            };
            _dbContext.Devices.Add(device);
            await _dbContext.SaveChangesAsync();
            //ErrorType.Success, 
            return Json(new AiurValue<long>(device.Id)
            {
                Code = ErrorType.Success,
                Message = "Successfully created your new device with id: " + device.Id
            });
        }

        [HttpPost]
        [APIProduces(typeof(AiurValue<Device>))]
        public async Task<IActionResult> UpdateDevice(UpdateDeviceAddressModel model)
        {
            var user = await GetKahlaUser();
            var device = await _dbContext
                .Devices
                .Where(t => t.UserId == user.Id)
                .SingleOrDefaultAsync(t => t.Id == model.DeviceId);
            if (device == null)
            {
                return this.Protocol(ErrorType.NotFound, "Can not find a device with ID: " + model.DeviceId);
            }
            device.Name = model.Name;
            device.PushAuth = model.PushAuth;
            device.PushEndpoint = model.PushEndpoint;
            device.PushP256DH = model.PushP256DH;
            _dbContext.Devices.Update(device);
            await _dbContext.SaveChangesAsync();
            //ErrorType.Success, 
            return Json(new AiurValue<Device>(device)
            {
                Code = ErrorType.Success,
                Message = "Successfully updated your new device with id: " + device.Id
            });
        }

        [APIProduces(typeof(AiurCollection<Device>))]
        public async Task<IActionResult> MyDevices()
        {
            var user = await GetKahlaUser();
            var devices = await _dbContext
                .Devices
                .Where(t => t.UserId == user.Id)
                .OrderByDescending(t => t.AddTime)
                .ToListAsync();
            return Json(new AiurCollection<Device>(devices)
            {
                Code = ErrorType.Success,
                Message = "Successfully get all your devices."
            });
        }

        [HttpPost]
        public async Task<IActionResult> PushTestMessage()
        {
            var user = await GetKahlaUser();
            _dbContext.Entry(user)
                .Collection(b => b.HisDevices)
                .Load();
            var messageEvent = new NewMessageEvent
            {
                Message = new Message
                {
                    ConversationId = -1,
                    Sender = new KahlaUser
                    {
                        IconFilePath = "kahla-user-icon/logo.png",
                        NickName = "Aiursoft Push System",
                    },
                    SenderId = "<Example user>",
                    Content = "U2FsdGVkX1+6kWGFqiSsjuPWX2iS7occQbqXm+PCNDLleTdk5p2UVQgQpu8J4XAYSpz/NT6N5mJMUQIUrNt6Ow==",
                    SendTime = DateTime.UtcNow,
                },
                AESKey = "37316f609ebc4e79bd7812a5f2ab37b8",
                Muted = false,
                Mentioned = false
            };
            var payload = JsonConvert.SerializeObject(messageEvent, Formatting.Indented, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            var token = await _appsContainer.AccessToken();
            await _thirdPartyPushService.PushAsync(user.HisDevices, "postermaster@aiursoft.com", payload);
            await _stargatePushService.PushMessageAsync(token, user.CurrentChannel, payload);
            return this.Protocol(ErrorType.Success, "Successfully sent you a test message to all your devices.");
        }

        private Task<KahlaUser> GetKahlaUser() => _userManager.GetUserAsync(User);
    }
}
