using Aiursoft.AiurObserver.WebSocket.Server;
using Aiursoft.AiurProtocol.Models;
using Aiursoft.AiurProtocol.Server.Attributes;
using Aiursoft.Kahla.Server.Attributes;
using Aiursoft.Kahla.Server.Data;
using Microsoft.AspNetCore.Mvc;
using Aiursoft.AiurObserver.Extensions;
using Aiursoft.AiurProtocol.Server;
using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Models.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace Aiursoft.Kahla.Server.Controllers;

[ApiExceptionHandler(
    PassthroughRemoteErrors = true,
    PassthroughAiurServerException = true)]
[ApiModelStateChecker]
[Route("api/messages")]
public class MessageController(
    InMemoryDataContext context,
    KahlaDbContext dbContext,
    ILogger<MessageController> logger,
    UserManager<KahlaUser> userManager) : ControllerBase
{
    [KahlaForceAuth]
    [Route("init-websocket")]
    public async Task<IActionResult> InitWebSocket()
    {
        var user = await this.GetCurrentUser(userManager);
        logger.LogInformation("User with email: {Email} is trying to init a websocket OTP.", user.Email);
        var otp = Guid.NewGuid().ToString("N");
        var otpValidTo = DateTime.UtcNow.AddMinutes(5);
        user.PushOtp = otp;
        user.PushOtpValidTo = otpValidTo;
        await userManager.UpdateAsync(user);
        return this.Protocol(new InitPusherViewModel
        {
            Code = Code.ResultShown,
            Message = "Successfully generated a new OTP. It will be valid for 5 minutes.",
            Otp = otp,
            OtpValidTo = otpValidTo,
            WebSocketEndpoint = $"{HttpContext.Request.Scheme.Replace("http", "ws")}://{HttpContext.Request.Host}/api/messages/websocket/{user.Id}?otp={otp}"
        });
    }

    [Route("websocket/{userId}")]
    public async Task<IActionResult> WebSocket([FromRoute] string userId, [FromQuery] string otp)
    {
        var user = await dbContext.Users.FindAsync(userId);
        if (user == null)
        {
            logger.LogWarning("User with ID: {UserId} is trying to get a websocket but the user does not exist.",
                userId);
            return this.Protocol(Code.NotFound, "The target user does not exist.");
        }

        if (!string.Equals(user.PushOtp, otp, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("User with email: {Email} is trying to get a websocket with invalid OTP {HisOTP}.",
                user.Email, otp);
            return this.Protocol(Code.Unauthorized, "Invalid OTP.");
        }
        
        if (user.PushOtpValidTo < DateTime.UtcNow)
        {
            logger.LogWarning("User with email: {Email} is trying to get a websocket with expired OTP {HisOTP}.",
                user.Email, otp);
            return this.Protocol(Code.Unauthorized, "Expired OTP.");
        }

        logger.LogInformation("User with email: {Email} is trying to get a websocket. And he provided the correct OTP.",
            user.Email);
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
            logger.LogInformation("User with email: {Email} closed the websocket.", user.Email);
            await pusher.Close(HttpContext.RequestAborted);
            outSub.Unsubscribe();
        }

        return new EmptyResult();
    }
}