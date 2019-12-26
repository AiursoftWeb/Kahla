using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Kahla.SDK.Attributes
{
    public class OnlineDetector : ActionFilterAttribute
    {
        public static object _obj = new object();
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var cache = context.HttpContext.RequestServices.GetService(typeof(IMemoryCache)) as IMemoryCache;
            if (context.HttpContext.User.Identity.IsAuthenticated)
            {
                var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrWhiteSpace(userId))
                {
                    lock (_obj)
                    {
                        cache.Set(userId, DateTime.UtcNow);
                    }
                }
            }
        }
    }
}
