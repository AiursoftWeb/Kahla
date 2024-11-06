using System.Security.Cryptography;
using Aiursoft.AiurObserver.WebSocket.Server;
using Aiursoft.AiurProtocol.Models;
using Aiursoft.AiurProtocol.Server.Attributes;
using Aiursoft.Kahla.Server.Attributes;
using Aiursoft.Kahla.Server.Data;
using Microsoft.AspNetCore.Mvc;
using Aiursoft.AiurObserver.Extensions;
using Aiursoft.AiurProtocol.Server;
using Aiursoft.Kahla.SDK.Models.Entities;
using Aiursoft.Kahla.SDK.Models.ViewModels;
using Aiursoft.WebTools.Attributes;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;

namespace Aiursoft.Kahla.Server.Controllers;

[LimitPerMin]
[ApiExceptionHandler(
    PassthroughRemoteErrors = true,
    PassthroughAiurServerException = true)]
[ApiModelStateChecker]
[Route("api/messages")]
public class MessageController(
    IDataProtectionProvider dataProtectionProvider,
    InMemoryDataContext context,
    KahlaDbContext dbContext,
    ILogger<MessageController> logger,
    UserManager<KahlaUser> userManager) : ControllerBase
{
    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector("WebSocketOTP");
    
    [KahlaForceAuth]
    [Route("init-websocket")]
    public async Task<IActionResult> InitWebSocket()
    {
        var user = await this.GetCurrentUser(userManager);
        logger.LogInformation("User with Id: {Id} is trying to init a websocket OTP.", user.Email);
        var validTo = DateTime.UtcNow.AddMinutes(5);
        var otpRaw = $"uid={user.Id},vlt={validTo}";
        var protectedOtp = _protector.Protect(otpRaw);
        return this.Protocol(new InitPusherViewModel
        {
            Code = Code.ResultShown,
            Message = "Successfully generated a new OTP. It will be valid for 5 minutes.",
            Otp = protectedOtp,
            WebSocketEndpoint = $"{HttpContext.Request.Scheme.Replace("http", "ws")}://{HttpContext.Request.Host}/api/messages/websocket/{user.Id}?otp={protectedOtp}"
        });
    }

    [EnforceWebSocket]
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

        try
        {
            var otpRaw = _protector.Unprotect(otp);
            var parts = otpRaw.Split(',');
            var userIdInOtp = parts[0].Split('=')[1];
            var validTo = DateTime.Parse(parts[1].Split('=')[1]);
            if (userIdInOtp != userId)
            {
                logger.LogWarning("User with ID: {UserId} is trying to get a websocket with invalid OTP {HisOTP}.",
                    userId, otp);
                return this.Protocol(Code.Unauthorized, "Invalid OTP.");
            }
            if (validTo < DateTime.UtcNow)
            {
                logger.LogWarning("User with ID: {UserId} is trying to get a websocket with expired OTP {HisOTP}.",
                    userId, otp);
                return this.Protocol(Code.Unauthorized, "Expired OTP.");
            }
        }
        catch (CryptographicException)
        {
            logger.LogWarning("User with ID: {UserId} is trying to get a websocket with invalid OTP {HisOTP}.",
                userId, otp);
            return this.Protocol(Code.Unauthorized, "Invalid OTP.");
        }

        logger.LogInformation("User with Id: {Id} is trying to get a websocket. And he provided the correct OTP.",
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
            logger.LogInformation("User with Id: {Id} closed the websocket.", user.Email);
            await pusher.Close(HttpContext.RequestAborted);
            outSub.Unsubscribe();
        }

        return new EmptyResult();
    }
}