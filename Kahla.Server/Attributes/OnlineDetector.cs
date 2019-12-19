using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Security.Claims;

namespace Kahla.Server.Attributes
{
    public class OnlineDetector : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.User.Identity.IsAuthenticated)
            {
                var cache = context.HttpContext.RequestServices.GetService<MemoryCache>();
                var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (cache != null && !string.IsNullOrWhiteSpace(userId))
                {
                    cache.Set($"last-call-user-{userId}", DateTime.UtcNow);
                }
            }
        }
    }
}
