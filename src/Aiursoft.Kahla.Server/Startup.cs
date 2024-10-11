using Aiursoft.AiurProtocol.Server;
using Aiursoft.Canon;
using Aiursoft.DocGenerator.Services;
using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.Server.Attributes;
using Aiursoft.Kahla.Server.Data;
using Aiursoft.Kahla.Server.Middlewares;
using Aiursoft.Kahla.Server.Services;
using Aiursoft.Kahla.Server.Services.AppService;
using Aiursoft.Kahla.Server.Services.Mappers;
using Aiursoft.Kahla.Server.Services.Repositories;
using Aiursoft.WebTools.Abstractions.Models;
using Microsoft.AspNetCore.Identity;
using WebPush;

namespace Aiursoft.Kahla.Server
{
    public class Startup : IWebStartup
    {
        public void ConfigureServices(IConfiguration configuration, IWebHostEnvironment environment, IServiceCollection services)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            // Database
            services.AddMemoryCache();
            services.AddSingleton<InMemoryDataContext>();
            services.AddDatabase(connectionString);
            
            // Identity
            services.AddIdentity<KahlaUser, IdentityRole>(options => options.Password = new PasswordOptions
                {
                    RequireNonAlphanumeric = false,
                    RequireDigit = false,
                    RequiredLength = 6,
                    RequiredUniqueChars = 0,
                    RequireLowercase = false,
                    RequireUppercase = false
                })
                .AddEntityFrameworkStores<KahlaDbContext>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(t => t.Cookie.SameSite = SameSiteMode.None);
            
            // Repositories
            services.AddScoped<UserOthersViewRepo>();
            services.AddScoped<ThreadOthersViewRepo>();
            services.AddScoped<ThreadJoinedViewRepo>();
            
            // App services
            services.AddScoped<UserOthersViewAppService>();
            services.AddScoped<UserDetailedViewAppService>();
            services.AddScoped<ThreadOthersViewAppService>();
            services.AddScoped<ThreadJoinedViewAppService>();
            
            // Services
            services.AddScoped<WebPushClient>();
            services.AddScoped<WebPushService>();
            services.AddScoped<WebSocketPushService>();
            services.AddScoped<KahlaPushService>();
            services.AddScoped<KahlaMapper>();
            services.AddScoped<OnlineJudger>();

            services.AddTaskCanon();

            services
                .AddControllers()
                .AddApplicationPart(typeof(Startup).Assembly)
                .AddAiurProtocol();
        }

        public void Configure(WebApplication app)
        {
            app.UseWebSockets();
            app.UseMiddleware<HandleKahlaOptionsMiddleware>();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<OnlineDetectorMiddleware>();
            app.MapDefaultControllerRoute();
            app.UseAiursoftDocGenerator(options =>
            {
                options.DocAddress = "/api/doc";
                options.Format = DocFormat.Markdown;
                options.RequiresAuthorized = (action, controller) =>
                {
                    return
                        action.CustomAttributes.Any(t => t.AttributeType == typeof(KahlaForceAuth)) ||
                        controller.CustomAttributes.Any(t => t.AttributeType == typeof(KahlaForceAuth));
                };
            });

        }
    }
}
