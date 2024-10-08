using Aiursoft.AiurProtocol.Models;
using Aiursoft.AiurProtocol.Server;
using Aiursoft.AiurProtocol.Server.Attributes;
using Aiursoft.Canon;
using Aiursoft.DocGenerator.Attributes;
using Aiursoft.Kahla.SDK.Events;
using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Models.AddressModels;
using Aiursoft.Kahla.SDK.ModelsOBS;
using Aiursoft.Kahla.Server.Attributes;
using Aiursoft.Kahla.Server.Data;
using Aiursoft.Kahla.Server.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Controllers;

[KahlaForceAuth]
[GenerateDoc]
[ApiExceptionHandler(
    PassthroughRemoteErrors = true,
    PassthroughAiurServerException = true)]
[ApiModelStateChecker]
[Route("api/devices")]
public class DevicesController(
    WebPushService webPusher,
    WebSocketPushService wsPusher,
    CanonPool canonPool, // Transient service.
    ILogger<DevicesController> logger,
    UserManager<KahlaUser> userManager,
    KahlaDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [Route("my-devices")]
    public async Task<IActionResult> MyDevices()
    {
        var user = await this.GetCurrentUser(userManager);
        logger.LogInformation("User with email: {Email} is trying to get all his devices.", user.Email);
        var devices = await dbContext
            .Devices
            .AsNoTracking()
            .Where(t => t.OwnerId == user.Id)
            .OrderByDescending(t => t.AddTime)
            .ToListAsync();
        return this.Protocol(Code.ResultShown, "Successfully get all your devices.", devices);
    }

    [HttpPost]
    [Route("add-device")]
    public async Task<IActionResult> AddDevice(AddDeviceAddressModel model)
    {
        var user = await this.GetCurrentUser(userManager);
        var existingDevice = await dbContext.Devices.FirstOrDefaultAsync(t => t.PushP256Dh == model.PushP256Dh);
        if (existingDevice != null)
        {
            logger.LogInformation(
                "User with email: {Email} is trying to add a device that already exists. It's ID is: {DeviceId}",
                user.Email, existingDevice.Id);
            dbContext.Devices.Remove(existingDevice);
            await dbContext.SaveChangesAsync();
        }

        var devicesExists = await dbContext.Devices.Where(t => t.OwnerId == user.Id).ToListAsync();
        if (devicesExists.Count >= 20)
        {
            var toDrop = devicesExists.OrderBy(t => t.AddTime).First();
            logger.LogWarning(
                "User with email: {Email} is trying to add a device but he already has 20 devices! Trying to delete the oldest one with id: {DeviceId}",
                user.Email, toDrop.Id);
            dbContext.Devices.Remove(toDrop);
            await dbContext.SaveChangesAsync();
        }

        var device = new Device
        {
            Name = model.Name!,
            OwnerId = user.Id,
            PushAuth = model.PushAuth!,
            PushEndpoint = model.PushEndpoint!,
            PushP256Dh = model.PushP256Dh!,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()!
        };
        await dbContext.Devices.AddAsync(device);
        await dbContext.SaveChangesAsync();
        logger.LogInformation("User with email: {Email} successfully added a new device with id: {DeviceId}",
            user.Email, device.Id);
        return this.Protocol(Code.JobDone, "Successfully created your new device with id: " + device.Id,
            value: device.Id);
    }

    [HttpPost]
    [Route("drop-device/{id:int}")]
    public async Task<IActionResult> DropDevice([FromRoute] int id)
    {
        var user = await this.GetCurrentUser(userManager);
        logger.LogInformation("User with email: {Email} is trying to drop a device with id: {DeviceId}", user.Email, id);
        var device = await dbContext
            .Devices
            .Where(t => t.OwnerId == user.Id)
            .SingleOrDefaultAsync(t => t.Id == id);
        if (device == null)
        {
            return this.Protocol(Code.NotFound, $"Can't find your device with id: '{id}'.");
        }

        dbContext.Devices.Remove(device);
        await dbContext.SaveChangesAsync();
        logger.LogInformation("User with email: {Email} successfully dropped a device with id: {DeviceId}",
            user.Email, device.Id);
        return this.Protocol(Code.JobDone, $"Successfully dropped your device with id: '{id}'.");
    }

    [HttpPut]
    [Route("update-device/{id:int}")]
    public async Task<IActionResult> UpdateDevice([FromRoute] int id, AddDeviceAddressModel model)
    {
        var user = await this.GetCurrentUser(userManager);
        logger.LogInformation("User with email: {Email} is trying to patch a device with id: {DeviceId}", user.Email, id);
        var device = await dbContext
            .Devices
            .Where(t => t.OwnerId == user.Id)
            .SingleOrDefaultAsync(t => t.Id == id);
        if (device == null)
        {
            return this.Protocol(Code.NotFound, "Can not find a device with ID: " + id);
        }

        device.Name = model.Name!;
        device.PushAuth = model.PushAuth!;
        device.PushEndpoint = model.PushEndpoint!;
        device.PushP256Dh = model.PushP256Dh!;
        dbContext.Devices.Update(device);
        await dbContext.SaveChangesAsync();
        logger.LogInformation("User with email: {Email} successfully patched a device with id: {DeviceId}",
            user.Email, device.Id);
        return this.Protocol(Code.JobDone, "Successfully updated your new device with id: " + device.Id,
            value: device.Id);
    }

    [HttpPost]
    [Route("push-test-message")]
    public async Task<IActionResult> PushTestMessage()
    {
        var user = await this.GetCurrentUser(userManager);
        logger.LogInformation("User with email: {Email} is trying to push a test message to all his devices.", user.Email);
        await dbContext.Entry(user)
            .Collection(b => b.HisDevices)
            .LoadAsync();
        var messageEvent = new NewMessageEvent
        {
            Message = new Message
            {
                ConversationId = -1,
                Sender = new KahlaUser
                {
                    IconFilePath = null,
                    NickName = "Aiursoft Push System",
                },
                SenderId = "<Example user>",
                Content = "Sample message",
                SendTime = DateTime.UtcNow,
            },
            Muted = false,
        };
        
        canonPool.RegisterNewTaskToPool(async () => { await wsPusher.PushAsync(user, messageEvent); });
        foreach (var hisDevice in user.HisDevices)
        {
            canonPool.RegisterNewTaskToPool(async () => { await webPusher.PushAsync(hisDevice, messageEvent); });
        }

        await canonPool.RunAllTasksInPoolAsync(Environment
            .ProcessorCount); // Execute tasks in pool, running tasks should be max at 8.

        logger.LogInformation("User with email: {Email} successfully pushed a test message to all his devices.", user.Email);
        return this.Protocol(Code.JobDone, "Successfully sent you a test message to all your devices.");
    }
}