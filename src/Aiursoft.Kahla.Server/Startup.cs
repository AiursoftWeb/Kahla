using Aiursoft.Identity;
using Aiursoft.SDK;
using Aiursoft.Stargate.SDK;
using Aiursoft.WebTools.Abstractions.Models;
using Kahla.SDK.Models;
using Kahla.Server.Data;
using Kahla.Server.Middlewares;
using Microsoft.AspNetCore.Identity;
using WebPush;

namespace Kahla.Server
{
    public class Startup : IWebStartup
    {
        public void ConfigureServices(IConfiguration configuration, IWebHostEnvironment environment, IServiceCollection services)
        {
            services.AddDbContextForInfraApps<KahlaDbContext>(configuration.GetConnectionString("DatabaseConnection"));

            services.AddIdentity<KahlaUser, IdentityRole>()
                .AddEntityFrameworkStores<KahlaDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<List<DomainSettings>>(configuration.GetSection("AppDomain"));

            services.ConfigureApplicationCookie(t => t.Cookie.SameSite = SameSiteMode.None);
            
            services.AddAiursoftWebFeatures();
            services.AddAiursoftIdentity<KahlaUser>(
                probeConfig: configuration.GetSection("AiursoftProbe"),
                authenticationConfig: configuration.GetSection("AiursoftAuthentication"),
                observerConfig: configuration.GetSection("AiursoftObserver"));
            services.AddAiursoftStargate(configuration.GetSection("AiursoftStargate"));
            services.AddScoped<WebPushClient>();
        }

        public void Configure(WebApplication app)
        {
            app.UseMiddleware<HandleKahlaOptionsMiddleware>();
            app.UseAiursoftHandler(allowCors: false);
            app.UseAiursoftAPIAppRouters(true, t => 
            {
                 t.UseMiddleware<OnlineDetectorMiddleware>();
                 return t;
            });
        }
    }
}
