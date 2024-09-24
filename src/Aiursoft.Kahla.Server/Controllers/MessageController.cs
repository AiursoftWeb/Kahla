using Aiursoft.AiurObserver.WebSocket.Server;
using Aiursoft.AiurProtocol.Exceptions;
using Aiursoft.AiurProtocol.Models;
using Aiursoft.AiurProtocol.Server.Attributes;
using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.Server.Attributes;
using Aiursoft.Kahla.Server.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Aiursoft.AiurObserver.Extensions;

namespace Aiursoft.Kahla.Server.Controllers;

[KahlaForceAuth]
[ApiExceptionHandler(
    PassthroughRemoteErrors = true,
    PassthroughAiurServerException = true)]
[ApiModelStateChecker]
[Route("api/messages")]
public class MessageController(
    InMemoryDataContext context,
    ILogger<MessageController> logger,
    UserManager<KahlaUser> userManager) : ControllerBase
{
    [Route("{id}")]
    public async Task WebSocket([FromRoute]string id)
    {
        var user = await GetCurrentUser();
        logger.LogInformation("User with email: {Email} is trying to get a websocket.", user.Email);
        var pusher = await HttpContext.AcceptWebSocketClient();
        var channel = context.GetMyChannel(user.Id);
        var outSub = channel.Subscribe(t => pusher.Send(t, HttpContext.RequestAborted));
        
        try
        {
            await pusher.Listen(HttpContext.RequestAborted);
        }
        catch (TaskCanceledException)
        {
            // Ignore. This happens when the client closes the connection.
        }
        finally
        {
            await pusher.Close(HttpContext.RequestAborted);
            outSub.Unsubscribe();
        }
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