using Aiursoft.AiurProtocol.Exceptions;
using Aiursoft.AiurProtocol.Models;
using Aiursoft.AiurProtocol.Server;
using Aiursoft.AiurProtocol.Server.Attributes;
using Aiursoft.DocGenerator.Attributes;
using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Models.AddressModels;
using Aiursoft.Kahla.Server.Attributes;
using Aiursoft.Kahla.Server.Data;
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
    ILogger<DevicesController> logger,
    UserManager<KahlaUser> userManager, 
    KahlaDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [Route("my-devices")]
    public async Task<IActionResult> MyDevices()
    {
        var user = await GetCurrentUser();
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
        var user = await GetCurrentUser();
        var existingDevice = await dbContext.Devices.FirstOrDefaultAsync(t => t.PushP256Dh == model.PushP256Dh);
        if (existingDevice != null)
        {
            logger.LogInformation("User with email: {Email} is trying to add a device that already exists. It's ID is: {DeviceId}", user.Email, existingDevice.Id);
            dbContext.Devices.Remove(existingDevice);
            await dbContext.SaveChangesAsync();
        }
        var devicesExists = await dbContext.Devices.Where(t => t.OwnerId == user.Id).ToListAsync();
        if (devicesExists.Count >= 20)
        {
            var toDrop = devicesExists.OrderBy(t => t.AddTime).First();
            logger.LogWarning("User with email: {Email} is trying to add a device but he already has 20 devices! Trying to delete the oldest one with id: {DeviceId}", user.Email, toDrop.Id);
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
        logger.LogInformation("User with email: {Email} successfully added a new device with id: {DeviceId}", user.Email, device.Id);
        return this.Protocol(Code.JobDone, "Successfully created your new device with id: " + device.Id, value: device.Id);
    }
    
    private async Task<KahlaUser> GetCurrentUser()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            throw new AiurServerException(Code.Conflict, "The user you signed in was deleted from the database!");
        }
        return user;
    }
}