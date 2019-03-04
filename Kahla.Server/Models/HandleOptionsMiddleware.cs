using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Kahla.Server.Models
{
    public class HandleOptionsMiddleware
    {
        private readonly RequestDelegate _next;

        public HandleOptionsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Response.Headers.Add("Access-Control-Allow-Headers", "Authorization");
            context.Response.Headers.Add("Cache-Control", "no-cache");
            context.Response.Headers.Add("Expires", "-1");
            if (context.Request.Path.Value.ToLower().Contains("debug"))
            {
                context.Response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:8001");
                context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
            }
            else
            {
                context.Response.Headers.Add("Access-Control-Allow-Origin", "https://kahla.app.aiursoft.com");
                context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
            }
            if (context.Request.Method == "OPTIONS")
            {
                context.Response.StatusCode = 204;
                return;
            }
            await _next.Invoke(context);
        }
    }
}
