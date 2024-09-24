using Aiursoft.AiurProtocol.Models;
using Aiursoft.AiurProtocol.Server;
using Aiursoft.AiurProtocol.Server.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Aiursoft.Kahla.Server.Controllers;

[ApiExceptionHandler(
    PassthroughRemoteErrors = true, 
    PassthroughAiurServerException = true)]
[ApiModelStateChecker]
[Route("api")]
public class HomeController(
    ILogger<HomeController> logger) : ControllerBase
{
    public IActionResult Index()
    {
        logger.LogInformation("User with IP address {IP} visited the home page.", HttpContext.Connection.RemoteIpAddress);
        return this.Protocol(Code.ResultShown, "Welcome to this API project!");
    }
}