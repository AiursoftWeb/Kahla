using Aiursoft.AiurProtocol.Exceptions;
using Aiursoft.AiurProtocol.Models;
using Aiursoft.AiurProtocol.Server;
using Aiursoft.AiurProtocol.Server.Attributes;
using Aiursoft.DocGenerator.Attributes;
using Aiursoft.Kahla.SDK.Models.AddressModels;
using Aiursoft.Kahla.SDK.Models.ViewModels;
using Aiursoft.Kahla.SDK.ModelsOBS;
using Aiursoft.Kahla.Server.Attributes;
using Aiursoft.Kahla.Server.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Controllers;

[GenerateDoc]
[ApiExceptionHandler(
    PassthroughRemoteErrors = true, 
    PassthroughAiurServerException = true)]
[ApiModelStateChecker]
[Route("api/auth")]
public class AuthController(
    KahlaDbContext dbContext,
    UserManager<KahlaUser> userManager,
    SignInManager<KahlaUser> signInManager,
    ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost]
    [Route("signin")]
    public async Task<IActionResult> SignIn(SignInAddressModel model)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return this.Protocol(Code.Conflict, "You are already signed in!");
        }
        var result = await signInManager.PasswordSignInAsync(model.Email!, model.Password!, true, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            logger.LogInformation("User with email: {Email} logged in.", model.Email);
            return this.Protocol(Code.JobDone, "User logged in!");
        }
        if (result.IsLockedOut)
        {
            logger.LogWarning("User account locked out with email: {Email}", model.Email);
            return this.Protocol(Code.NoActionTaken, "User account locked out!");
        }
        else
        {
            logger.LogWarning("Invalid login attempt from email: {Email}", model.Email);
            return this.Protocol(Code.Unauthorized, "Invalid login attempt! Please check your email and password.");
        }
    }
    
    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register(RegisterAddressModel model)
    {
        var user = new KahlaUser
        {
            UserName = model.Email,
            Email = model.Email,
            NickName = model.Email!.Split('@')[0]
        };
        var result = await userManager.CreateAsync(user, model.Password!);
        if (result.Succeeded)
        {
            await signInManager.SignInAsync(user, isPersistent: false);
            logger.LogInformation("User with email: {Email} created.", model.Email);
            return this.Protocol(Code.JobDone, "User created!");
        }
        
        logger.LogWarning("Failed to create user with email: {Email}. Errors: {Errors}", model.Email, result.Errors);
        return this.Protocol(Code.Conflict, string.Join(", ", result.Errors.Select(t => t.Description)));
    }
    
    [KahlaForceAuth]
    [HttpPost]
    [Route("signout")]
    public async Task<IActionResult> SignOutUser(SignOutAddressModel model)
    {
        var user = await GetCurrentUser();
        logger.LogInformation("User with email: {Email} requested to sign out.", user.Email);
        var device = await dbContext
            .Devices
            .Where(t => t.UserId == user.Id)
            .SingleOrDefaultAsync(t => t.Id == model.DeviceId);
        
        await signInManager.SignOutAsync();
        if (device == null)
        {
            logger.LogWarning(
                "User with email: {Email} signed out, but we did not find device with id: {DeviceId}. It is suggested to pass the 'deviceid' parameter so we will remove the device from your account.",
                user.Email, model.DeviceId);
            return this.Protocol(Code.JobDone,
                "Successfully signed you off, but we could not find device with id: " + model.DeviceId +" in your account.");
        }

        dbContext.Devices.Remove(device);
        await dbContext.SaveChangesAsync();
        logger.LogInformation("User with email: {Email} signed out and removed device with id: {DeviceId}.", user.Email, model.DeviceId);
        return this.Protocol(Code.JobDone, "Success. And the device with id: " + model.DeviceId + " is removed from your account.");
    }
    
    [KahlaForceAuth]
    [HttpGet]
    [Route("me")]
    public async Task<IActionResult> Me()
    {
        var user = await GetCurrentUser();
        logger.LogInformation("User with email: {Email} queried their own information.", user.Email);
        return this.Protocol(new MeViewModel
        {
            Code = Code.ResultShown,
            Message = "Got your user!",
            User = user
        });
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