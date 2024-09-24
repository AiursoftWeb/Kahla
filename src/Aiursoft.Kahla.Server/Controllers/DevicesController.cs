using Aiursoft.AiurProtocol.Exceptions;
using Aiursoft.AiurProtocol.Models;
using Aiursoft.AiurProtocol.Server;
using Aiursoft.AiurProtocol.Server.Attributes;
using Aiursoft.DocGenerator.Attributes;
using Aiursoft.Kahla.SDK.Models;
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