using Microsoft.Extensions.Caching.Memory;

namespace Aiursoft.Kahla.Server.Middlewares;

public class OnlineDetectorMiddleware(
    ILogger<OnlineDetectorMiddleware> logger,
    RequestDelegate next,
    IMemoryCache memoryCache)
{
    private static readonly object Obj = new();

    public async Task Invoke(HttpContext context)
    {
        if (context.User.Identity is { IsAuthenticated: true })
        {
            var userId = context.User.GetUserId();
            if (!string.IsNullOrWhiteSpace(userId))
            {
                logger.LogInformation($"User with ID {userId} from IP {context.Connection.RemoteIpAddress} is calling an API. Mark him as online.");
                lock (Obj)
                {
                    memoryCache.Set($"last-access-time-{userId}", DateTime.UtcNow);
                }
            }
        }
        await next.Invoke(context);
    }
}