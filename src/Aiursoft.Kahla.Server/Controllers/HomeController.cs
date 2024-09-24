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
[Route("api")]
public class HomeController(
    UserManager<KahlaUser> userManager,
    SignInManager<KahlaUser> signInManager,
    ILogger<HomeController> logger) : ControllerBase
{
    public IActionResult Index()
    {
        return this.Protocol(Code.ResultShown, "Welcome to this API project!");
    }
    
    [HttpPost]
    [Route("signin")]
    public async Task<IActionResult> SignIn(SignInAddressModel model)
    {
        var result = await signInManager.PasswordSignInAsync(model.Email!, model.Password!, true, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            logger.LogInformation(1, "User logged in");
            return this.Protocol(Code.JobDone, "User logged in!");
        }
        if (result.IsLockedOut)
        {
            logger.LogWarning(2, "User account locked out");
            return this.Protocol(Code.NoActionTaken, "User account locked out!");
        }
        else
        {
            logger.LogWarning(3, "Invalid login attempt");
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
        return this.Protocol(Code.InvalidInput, "Failed to create user!", result.Errors.ToArray());
    }
    
    [Authorize]
    [HttpPost]
    [Route("signout")]
    public async Task<IActionResult> SignOutUser()
    {
        await signInManager.SignOutAsync();
        return this.Protocol(Code.JobDone, "User signed out!");
    }
    
    [Authorize]
    [HttpGet]
    [Route("me")]
    public async Task<IActionResult> Me()
    {
        var user = await GetCurrentUser();
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