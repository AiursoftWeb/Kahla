using Aiursoft.AiurProtocol.Server;
using Aiursoft.DbTools.Sqlite;
using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.Server.Data;
using Aiursoft.Kahla.Server.Middlewares;
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

            services.AddMemoryCache();
            services.AddAiurSqliteWithCache<KahlaDbContext>(connectionString);
            
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

            services.Configure<List<DomainSettings>>(configuration.GetSection("AppDomain"));
            services.ConfigureApplicationCookie(t => t.Cookie.SameSite = SameSiteMode.None);
            services.AddScoped<WebPushClient>();
            
            services
                .AddControllers()
                .AddApplicationPart(typeof(Startup).Assembly)
                .AddAiurProtocol();
        }

        public void Configure(WebApplication app)
        {
            app.UseMiddleware<HandleKahlaOptionsMiddleware>();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapDefaultControllerRoute();
        }
    }
}
