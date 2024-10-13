using Aiursoft.AiurProtocol.Models;
using Aiursoft.AiurProtocol.Server;
using Aiursoft.AiurProtocol.Server.Attributes;
using Aiursoft.DocGenerator.Attributes;
using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Models.AddressModels;
using Aiursoft.Kahla.SDK.Models.ViewModels;
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
            logger.LogWarning("User with Id: {Id} tried to sign in while he is already signed in.", model.Email);
            return this.Protocol(Code.Conflict, "You are already signed in!");
        }
        var result = await signInManager.PasswordSignInAsync(model.Email!, model.Password!, true, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            logger.LogInformation("User with Id: {Id} logged in.", model.Email);
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
        logger.LogInformation("User with Id: {Id} requested to register.", model.Email);
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
            logger.LogInformation("User with Id: {Id} created.", model.Email);
            return this.Protocol(Code.JobDone, "User created!");
        }
        
        logger.LogWarning("Failed to create User with Id: {Id}. Errors: {Errors}", model.Email, result.Errors);
        return this.Protocol(Code.Conflict, string.Join(", ", result.Errors.Select(t => t.Description)));
    }
    
    [KahlaForceAuth]
    [HttpPost]
    [Route("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordAddressModel model)
    {
        var currentUser = await this.GetCurrentUser(userManager);
        logger.LogInformation("User with Id: {Id} requested to change his password.", currentUser.Email);
        var result = await userManager.ChangePasswordAsync(currentUser, model.OldPassword!, model.NewPassword!);
        if (!result.Succeeded)
        {
            logger.LogWarning("Failed to change password for User with Id: {Id}. Errors: {Errors}", currentUser.Email, result.Errors);
            return this.Protocol(Code.Unauthorized, string.Join(", ", result.Errors.Select(t => t.Description)));
        }
        logger.LogInformation("User with Id: {Id} successfully changed his password.", currentUser.Email);
        return this.Protocol(Code.JobDone, "Successfully changed your password!");
    }
    
    [KahlaForceAuth]
    [HttpPost]
    [Route("signout")]
    public async Task<IActionResult> SignOutUser(SignOutAddressModel model)
    {
        var user = await this.GetCurrentUser(userManager);
        logger.LogInformation("User with Id: {Id} requested to sign out.", user.Email);
        var device = await dbContext
            .Devices
            .Where(t => t.OwnerId == user.Id)
            .SingleOrDefaultAsync(t => t.Id == model.DeviceId);
        
        await signInManager.SignOutAsync();
        if (device == null)
        {
            logger.LogWarning(
                "User with Id: {Id} signed out, but we did not find device with id: {DeviceId}. It is suggested to pass the 'deviceid' parameter so we will remove the device from your account.",
                user.Email, model.DeviceId);
            return this.Protocol(Code.JobDone,
                "Successfully signed you off, but we could not find device with id: " + model.DeviceId +" in your account.");
        }

        dbContext.Devices.Remove(device);
        await dbContext.SaveChangesAsync();
        logger.LogInformation("User with Id: {Id} signed out and removed device with id: {DeviceId}.", user.Email, model.DeviceId);
        return this.Protocol(Code.JobDone, "Success. And the device with id: " + model.DeviceId + " is removed from your account.");
    }
    
    [KahlaForceAuth]
    [HttpGet]
    [Route("me")]
    public async Task<IActionResult> Me()
    {
        var user = await this.GetCurrentUser(userManager);
        logger.LogInformation("User with Id: {Id} queried his own information.", user.Email);
        return this.Protocol(new MeViewModel
        {
            Code = Code.ResultShown,
            Message = "Got your user!",
            User = user,
            PrivateSettings = new PrivateSettings
            {
                ThemeId = user.ThemeId,
                AllowHardInvitation = user.AllowHardInvitation,
                EnableEmailNotification = user.EnableEmailNotification,
                AllowSearchByName = user.AllowSearchByName,
                EnableEnterToSendMessage = user.EnableEnterToSendMessage,
                EnableHideMyOnlineStatus = user.EnableHideMyOnlineStatus,
            }
        });
    }
    
    [KahlaForceAuth]
    [HttpPatch]
    [Route("update-me")]
    public async Task<IActionResult> UpdateClientSetting(UpdateMeAddressModel model)
    {
        var userTrackedInDb = await this.GetCurrentUser(userManager);
        logger.LogInformation("User with Id: {Id} is trying to update his client setting.", userTrackedInDb.Email);
        
        // Public information
        if (model.NickName != null)
        {
            userTrackedInDb.NickName = model.NickName;
        }
        if (model.Bio != null)
        {
            userTrackedInDb.Bio = model.Bio;
        }
        
        // Private information
        if (model.ThemeId.HasValue)
        {
            userTrackedInDb.ThemeId = model.ThemeId ?? 0;
        }
        if (model.AllowHardInvitation.HasValue)
        {
            userTrackedInDb.AllowHardInvitation = model.AllowHardInvitation == true;
        }
        if (model.EnableEmailNotification.HasValue)
        {
            userTrackedInDb.EnableEmailNotification = model.EnableEmailNotification == true;
        }
        if (model.AllowSearchByName.HasValue)
        {
            userTrackedInDb.AllowSearchByName = model.AllowSearchByName == true;
        }
        if (model.EnableEnterToSendMessage.HasValue)
        {
            userTrackedInDb.EnableEnterToSendMessage = model.EnableEnterToSendMessage == true;
        }
        if (model.EnableHideMyOnlineStatus.HasValue)
        {
            userTrackedInDb.EnableHideMyOnlineStatus = model.EnableHideMyOnlineStatus == true;
        }
        await userManager.UpdateAsync(userTrackedInDb);
        logger.LogInformation("User with Id: {Id} successfully updated his client setting.", userTrackedInDb.Email);
        return this.Protocol(Code.JobDone, "Successfully update your client setting. Please call the 'me' API to get the latest information.");
    }
}