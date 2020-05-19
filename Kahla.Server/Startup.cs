using Aiursoft.Archon.SDK.Services;
using Aiursoft.Pylon;
using Aiursoft.SDK;
using Kahla.SDK.Models;
using Kahla.Server.Data;
using Kahla.Server.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using WebPush;

namespace Kahla.Server
{
    public class Startup
    {
        private IConfiguration _configuration { get; }

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
            AppsContainer.CurrentAppId = configuration["KahlaAppId"];
            AppsContainer.CurrentAppSecret = configuration["KahlaAppSecret"];
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContextWithCache<KahlaDbContext>(_configuration.GetConnectionString("DatabaseConnection"));

            services.AddIdentity<KahlaUser, IdentityRole>()
                .AddEntityFrameworkStores<KahlaDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<List<DomainSettings>>(_configuration.GetSection("AppDomain"));

            services.ConfigureApplicationCookie(t => t.Cookie.SameSite = SameSiteMode.None);

            services.AddAiurAPIMvc();
            services.AddAiurDependenciesWithIdentity<KahlaUser>();
            services.AddScoped<WebPushClient>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMiddleware<HandleKahlaOptionsMiddleware>();
            app.UseAiurAPIHandler(env.IsDevelopment());
            app.UseAiursoftDefault(t => t.UseMiddleware<OnlineDetectorMiddleware>());
        }
    }
}
