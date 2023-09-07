using Aiursoft.Identity;
using Microsoft.Extensions.Caching.Memory;

namespace Kahla.Server.Middlewares
{
    public class OnlineDetectorMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _memoryCache;
        private static object _obj = new object();

        public OnlineDetectorMiddleware(
            RequestDelegate next,
            IMemoryCache memoryCache)
        {
            _next = next;
            _memoryCache = memoryCache;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
            {
                var userId = context.User.GetUserId();
                if (!string.IsNullOrWhiteSpace(userId))
                {
                    lock (_obj)
                    {
                        _memoryCache.Set($"last-access-time-{userId}", DateTime.UtcNow);
                    }
                }
            }
            await _next.Invoke(context);
        }
    }
}
