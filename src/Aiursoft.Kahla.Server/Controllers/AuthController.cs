using Aiursoft.AiurProtocol.Models;
using Aiursoft.AiurProtocol.Server;
using Aiursoft.AiurProtocol.Server.Attributes;
using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Models.ApiAddressModels;
using Aiursoft.Kahla.SDK.Models.ApiViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Aiursoft.Kahla.Server.Controllers;

[ApiExceptionHandler(
    PassthroughRemoteErrors = true, 
    PassthroughAiurServerException = true)]
[ApiModelStateChecker]
[Route("api/auth")]
public class AuthController(
    UserManager<KahlaUser> userManager,
    SignInManager<KahlaUser> signInManager,
    ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost]
    [Route("signin")]
    public async Task<IActionResult> SignIn(SignInAddressModel model)
    {
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
            return this.Protocol(Code.InvalidInput, "Invalid login attempt!");
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
            logger.LogInformation($"User {user.Email} created.");
            return this.Protocol(Code.JobDone, "User created!");
        }
        else
        {
            logger.LogWarning("Failed to create user with email: {Email}. Errors: {Errors}", model.Email, result.Errors);
        }
        return this.Protocol(Code.InvalidInput, "Failed to create user!", result.Errors.ToArray());
    }
    
    [Authorize]
    [HttpPost]
    [Route("signout")]
    public async Task<IActionResult> SignOutUser()
    {
        await signInManager.SignOutAsync();
        logger.LogInformation("User with email: {Email} signed out.", (await GetCurrentUser()).Email);
        return this.Protocol(Code.JobDone, "User signed out!");
    }
    
    [Authorize]
    [HttpGet]
    [Route("me")]
    public async Task<IActionResult> Me()
    {
        var user = await GetCurrentUser();
        logger.LogInformation("User with email: {Email} queried their own information.", user.Email);
        return this.Protocol( new MeViewModel
        {
            Code = Code.ResultShown,
            Message = "Got your user!",
            User = user
        });
    }

    private async Task<KahlaUser> GetCurrentUser()
    {
        var user = await userManager.GetUserAsync(User);
        return user;
    }
}