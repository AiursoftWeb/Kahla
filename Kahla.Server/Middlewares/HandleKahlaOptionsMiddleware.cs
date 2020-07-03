using Kahla.SDK.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.Server.Middlewares
{
    public class HandleKahlaOptionsMiddleware
    {
        private List<DomainSettings> AppDomain { get; }
        private readonly RequestDelegate _next;

        public HandleKahlaOptionsMiddleware(
            RequestDelegate next,
            IOptions<List<DomainSettings>> optionsAccessor)
        {
            AppDomain = optionsAccessor.Value;
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var settingsRecord = AppDomain.FirstOrDefault(t => t.Server.ToLower().Trim() == context.Request.Host.ToString().ToLower().Trim());
            context.Response.Headers.Add("Cache-Control", "no-cache");
            context.Response.Headers.Add("Expires", "-1");
            if (settingsRecord != null)
            {
                context.Response.Headers.Add("Access-Control-Allow-Origin", settingsRecord.Client);
            }
            context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
            context.Response.Headers.Add("Access-Control-Allow-Headers", "Authorization");
            if (context.Request.Method == "OPTIONS")
            {
                context.Response.StatusCode = 204;
                return;
            }
            await _next.Invoke(context);
        }
    }
}
