using Aiursoft.AiurProtocol;
using Aiursoft.AiurProtocol.Server;
using Aiursoft.Canon;
using Aiursoft.Directory.SDK.Services;
using Aiursoft.Identity.Attributes;
using Aiursoft.Kahla.SDK.Events;
using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Models.ApiAddressModels;
using Aiursoft.Kahla.Server.Data;
using Aiursoft.Kahla.Server.Services;
using Aiursoft.Stargate.SDK.Services.ToStargateServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Controllers
{
    [ApiModelStateChecker]
    [ApiExceptionHandler]
    [AiurForceAuth(directlyReject: true)]
    public class DevicesController : ControllerBase
    {
        private readonly KahlaDbContext _dbContext;
        private readonly UserManager<KahlaUser> _userManager;
        private readonly DirectoryAppTokenService _appsContainer;
        private readonly CanonService _cannonService;

        public DevicesController(
            KahlaDbContext dbContext,
            UserManager<KahlaUser> userManager,
            DirectoryAppTokenService appsContainer,
            CanonService cannonService)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _appsContainer = appsContainer;
            _cannonService = cannonService;
        }

        [HttpPost]
        [Produces(typeof(AiurValue<int>))]
        public async Task<IActionResult> AddDevice(AddDeviceAddressModel model)
        {
            var user = await GetKahlaUser();
            var existingDevice = await _dbContext.Devices.FirstOrDefaultAsync(t => t.PushP256DH == model.PushP256DH);
            if (existingDevice != null)
            {
                _dbContext.Devices.Remove(existingDevice);
                await _dbContext.SaveChangesAsync();
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
                IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            await _dbContext.Devices.AddAsync(device);
            await _dbContext.SaveChangesAsync();
            //ErrorType.Success, 
            return this.Protocol(new AiurValue<long>(device.Id)
            {
                Code = Code.JobDone,
                Message = "Successfully created your new device with id: " + device.Id
            });
        }

        [HttpPost]
        [Produces(typeof(AiurValue<Device>))]
        public async Task<IActionResult> UpdateDevice(UpdateDeviceAddressModel model)
        {
            var user = await GetKahlaUser();
            var device = await _dbContext
                .Devices
                .Where(t => t.UserId == user.Id)
                .SingleOrDefaultAsync(t => t.Id == model.DeviceId);
            if (device == null)
            {
                return this.Protocol(Code.NotFound, "Can not find a device with ID: " + model.DeviceId);
            }
            device.Name = model.Name;
            device.PushAuth = model.PushAuth;
            device.PushEndpoint = model.PushEndpoint;
            device.PushP256DH = model.PushP256DH;
            _dbContext.Devices.Update(device);
            await _dbContext.SaveChangesAsync();
            //ErrorType.Success, 
            return this.Protocol(new AiurValue<Device>(device)
            {
                Code = Code.JobDone,
                Message = "Successfully updated your new device with id: " + device.Id
            });
        }

        [Produces(typeof(AiurCollection<Device>))]
        public async Task<IActionResult> MyDevices()
        {
            var user = await GetKahlaUser();
            var devices = await _dbContext
                .Devices
                .AsNoTracking()
                .Where(t => t.UserId == user.Id)
                .OrderByDescending(t => t.AddTime)
                .ToListAsync();
            return this.Protocol(new AiurCollection<Device>(devices)
            {
                Code = Code.ResultShown,
                Message = "Successfully get all your devices."
            });
        }

        [HttpPost]
        public async Task<IActionResult> DropDevice(int id)
        {
            var user = await GetKahlaUser();
            var device = await _dbContext
                .Devices
                .Where(t => t.UserId == user.Id)
                .SingleOrDefaultAsync(t => t.Id == id);
            if (device == null)
            {
                return this.Protocol(Code.NotFound, $"Can't find your device with id: '{id}'.");
            }
            _dbContext.Devices.Remove(device);
            await _dbContext.SaveChangesAsync();
            return this.Protocol(Code.JobDone, $"Successfully dropped your device with id: '{id}'.");
        }

        [HttpPost]
        public async Task<IActionResult> PushTestMessage()
        {
            var user = await GetKahlaUser();
            await _dbContext.Entry(user)
                .Collection(b => b.HisDevices)
                .LoadAsync();
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
            var token = await _appsContainer.GetAccessTokenAsync();
            _cannonService.FireAsync<ThirdPartyPushService>(s => s.PushAsync(user.HisDevices, messageEvent));
            _cannonService.FireAsync<PushMessageService>(s => s.PushMessageAsync(token, user.CurrentChannel, messageEvent));
            return this.Protocol(Code.JobDone, "Successfully sent you a test message to all your devices.");
        }

        private Task<KahlaUser> GetKahlaUser() => _userManager.GetUserAsync(User);
    }
}
