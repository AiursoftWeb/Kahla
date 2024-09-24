using Aiursoft.AiurProtocol.Models;
using Aiursoft.AiurProtocol.Server;
using Aiursoft.AiurProtocol.Server.Attributes;
using Aiursoft.Kahla.SDK.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Aiursoft.Kahla.Server.Controllers;

[ApiExceptionHandler(
    PassthroughRemoteErrors = true, 
    PassthroughAiurServerException = true)]
[ApiModelStateChecker]
[Route("api")]
public class HomeController(
    IConfiguration configuration,
    ILogger<HomeController> logger) : ControllerBase
{
    public IActionResult Index()
    {
        logger.LogInformation("User with IP address {IP} visited the home page.", HttpContext.Connection.RemoteIpAddress);
        var model = new IndexViewModel
        {
            Code = Code.ResultShown,
            Message = "Welcome to this API project!",
            AutoAcceptRequests = bool.Parse(configuration["AutoAcceptRequests"]?? "false"),
            ServerName = configuration["ServerName"] ?? "Kahla Server",
            VapidPublicKey = configuration["VapidKeys:PublicKey"] ?? string.Empty
        };
        return this.Protocol( model);
    }
}