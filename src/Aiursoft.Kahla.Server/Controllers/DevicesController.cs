using Aiursoft.AiurProtocol.Models;
using Aiursoft.AiurProtocol.Server;
using Aiursoft.AiurProtocol.Server.Attributes;
using Aiursoft.DocGenerator.Attributes;
using Aiursoft.Kahla.SDK.Events;
using Aiursoft.Kahla.SDK.Models.AddressModels;
using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.Server.Attributes;
using Aiursoft.Kahla.Server.Data;
using Aiursoft.Kahla.Server.Models.Entities;
using Aiursoft.Kahla.Server.Services;
using Aiursoft.Kahla.Server.Services.Repositories;
using Aiursoft.WebTools.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Controllers;

[LimitPerMin]
[KahlaForceAuth]
[GenerateDoc]
[ApiExceptionHandler(
    PassthroughRemoteErrors = true,
    PassthroughAiurServerException = true)]
[ApiModelStateChecker]
[Route("api/devices")]
public class DevicesController(
    BufferedKahlaPushService kahlaPushService,
    ILogger<DevicesController> logger,
    DeviceOwnerViewRepo repo,
    KahlaRelationalDbContext relationalDbContext) : ControllerBase
{
    [HttpGet]
    [Route("my-devices")]
    public async Task<IActionResult> MyDevices()
    {
        var userId = User.GetUserId();
        logger.LogInformation("User with Id: {Id} is trying to get all his devices.", userId);
        var devices = await repo.SearchDevicesIOwn(userId).ToListAsync();
        return this.Protocol(Code.ResultShown, "Successfully get all your devices.", devices);
    }

    [HttpPost]
    [Route("add-device")]
    public async Task<IActionResult> AddDevice([FromForm]AddDeviceAddressModel model)
    {
        var userId = User.GetUserId();
        var existingDevice = await relationalDbContext.Devices.FirstOrDefaultAsync(t => t.PushP256Dh == model.PushP256Dh);
        if (existingDevice != null)
        {
            logger.LogInformation(
                "User with Id: {Id} is trying to add a device that already exists. It's ID is: {DeviceId}",
                userId, existingDevice.Id);
            relationalDbContext.Devices.Remove(existingDevice);
            await relationalDbContext.SaveChangesAsync();
        }

        var devicesExists = await relationalDbContext.Devices.Where(t => t.OwnerId == userId).ToListAsync();
        if (devicesExists.Count >= 20)
        {
            var toDrop = devicesExists.OrderBy(t => t.AddTime).First();
            logger.LogWarning(
                "User with Id: {Id} is trying to add a device but he already has 20 devices! Trying to delete the oldest one with id: {DeviceId}",
                userId, toDrop.Id);
            relationalDbContext.Devices.Remove(toDrop);
            await relationalDbContext.SaveChangesAsync();
        }

        var device = new Device
        {
            Name = model.Name,
            OwnerId = userId,
            PushAuth = model.PushAuth,
            PushEndpoint = model.PushEndpoint,
            PushP256Dh = model.PushP256Dh,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()!
        };
        await relationalDbContext.Devices.AddAsync(device);
        await relationalDbContext.SaveChangesAsync();
        logger.LogInformation("User with Id: {Id} successfully added a new device with id: {DeviceId}",
            userId, device.Id);
        return this.Protocol(Code.JobDone, "Successfully created your new device with id: " + device.Id,
            value: device.Id);
    }

    [HttpPost]
    [Route("drop-device/{id:int}")]
    public async Task<IActionResult> DropDevice([FromRoute] int id)
    {
        var userId = User.GetUserId();
        logger.LogInformation("User with Id: {Id} is trying to drop a device with id: {DeviceId}", userId, id);
        var device = await relationalDbContext
            .Devices
            .Where(t => t.OwnerId == userId)
            .SingleOrDefaultAsync(t => t.Id == id);
        if (device == null)
        {
            return this.Protocol(Code.NotFound, $"Can't find your device with id: '{id}'.");
        }

        relationalDbContext.Devices.Remove(device);
        await relationalDbContext.SaveChangesAsync();
        logger.LogInformation("User with Id: {Id} successfully dropped a device with id: {DeviceId}",
            userId, device.Id);
        return this.Protocol(Code.JobDone, $"Successfully dropped your device.");
    }

    [HttpPut]
    [Route("update-device/{id:int}")]
    public async Task<IActionResult> UpdateDevice([FromRoute] int id, [FromForm] AddDeviceAddressModel model)
    {
        var userId = User.GetUserId();
        logger.LogInformation("User with Id: {Id} is trying to patch a device with id: {DeviceId}", userId, id);
        var device = await relationalDbContext
            .Devices
            .Where(t => t.OwnerId == userId)
            .SingleOrDefaultAsync(t => t.Id == id);
        if (device == null)
        {
            return this.Protocol(Code.NotFound, "Can not find a device with ID: " + id);
        }

        device.Name = model.Name;
        device.PushAuth = model.PushAuth;
        device.PushEndpoint = model.PushEndpoint;
        device.PushP256Dh = model.PushP256Dh;
        relationalDbContext.Devices.Update(device);
        await relationalDbContext.SaveChangesAsync();
        logger.LogInformation("User with Id: {Id} successfully patched a device with id: {DeviceId}",
            userId, device.Id);
        return this.Protocol(Code.JobDone, "Successfully updated your new device with id: " + device.Id,
            value: device.Id);
    }

    [HttpPost]
    [Route("push-test-message")]
    public IActionResult PushTestMessage()
    {
        var currentUserId = User.GetUserId();
        logger.LogInformation("User with Id: {Id} is trying to push a test message to all his devices.", currentUserId);
        var messageEvent = new NewMessageEvent
        {
            Message = new KahlaMessageMappedSentView
            {
                Id = Guid.NewGuid(),
                ThreadId = -1,
                Sender = new KahlaUserMappedPublicView
                {
                    Id = currentUserId,
                    Bio = string.Empty,
                    NickName = "Aiursoft Push System",
                    IconFilePath = null,
                    AccountCreateTime = DateTime.MinValue,
                    Email = "no@domain.com",
                    EmailConfirmed = true
                },
                Preview = "Sample message",
                SendTime = DateTime.UtcNow,
            }
        };
        
        kahlaPushService.QueuePushEventToUser(currentUserId, PushMode.AllPath, messageEvent);

        logger.LogInformation("User with Id: {Id} successfully pushed a test message to all his devices.", currentUserId);
        return this.Protocol(Code.JobDone, "Successfully sent you a test message to all your devices.");
    }
}