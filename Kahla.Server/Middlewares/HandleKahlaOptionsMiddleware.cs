using Aiursoft.Pylon.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.Server.Middlewares
{
    public class HandleKahlaOptionsMiddleware
    {
        private IConfiguration _configuration { get; }
        private string _appDomain { get; }
        private RequestDelegate _next;

        public HandleKahlaOptionsMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _configuration = configuration;
            _appDomain = configuration["AppDomain"];
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Response.Headers.Add("Cache-Control", "no-cache");
            context.Response.Headers.Add("Expires", "-1");
            context.Response.Headers.Add("Access-Control-Allow-Origin", _appDomain);
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
