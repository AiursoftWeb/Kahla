using Aiursoft.Identity;
using Aiursoft.SDK;
using Aiursoft.Stargate.SDK;
using Kahla.SDK.Models;
using Kahla.Server.Data;
using Kahla.Server.Middlewares;
using Microsoft.AspNetCore.Identity;
using WebPush;

namespace Kahla.Server
{
    public class Startup
    {
        private IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContextWithCache<KahlaDbContext>(Configuration.GetConnectionString("DatabaseConnection"));

            services.AddIdentity<KahlaUser, IdentityRole>()
                .AddEntityFrameworkStores<KahlaDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<List<DomainSettings>>(Configuration.GetSection("AppDomain"));

            services.ConfigureApplicationCookie(t => t.Cookie.SameSite = SameSiteMode.None);
            
            services.AddAiurMvc();
            services.AddAiursoftIdentity<KahlaUser>(
                probeConfig: Configuration.GetSection("AiursoftProbe"),
                authenticationConfig: Configuration.GetSection("AiursoftAuthentication"),
                observerConfig: Configuration.GetSection("AiursoftObserver"));
            services.AddAiursoftStargate(Configuration.GetSection("AiursoftStargate"));
            services.AddScoped<WebPushClient>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMiddleware<HandleKahlaOptionsMiddleware>();
            app.UseAiursoftHandler(env.IsDevelopment(), allowCors: false);
            app.UseAiursoftAPIAppRouters(true, t => t.UseMiddleware<OnlineDetectorMiddleware>());
        }
    }
}
