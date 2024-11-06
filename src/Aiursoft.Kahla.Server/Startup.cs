using Aiursoft.AiurProtocol.Server;
using Aiursoft.ArrayDb.Partitions;
using Aiursoft.Canon;
using Aiursoft.DocGenerator.Services;
using Aiursoft.Kahla.SDK.Models.Entities;
using Aiursoft.Kahla.Server.Attributes;
using Aiursoft.Kahla.Server.Data;
using Aiursoft.Kahla.Server.Middlewares;
using Aiursoft.Kahla.Server.Services;
using Aiursoft.Kahla.Server.Services.AppService;
using Aiursoft.Kahla.Server.Services.Repositories;
using Aiursoft.WebTools.Abstractions.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
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
            services.AddSingleton<QuickMessageAccess>();
            services.AddRelationalDatabase(connectionString);
            
            // ArrayDb
            services.AddSingleton<ArrayDbContext>();
            services.AddSingleton<PartitionedObjectBucket<MessageInDatabaseEntity, int>>(_ =>
            {
                var dbPath = Path.Combine(configuration["Storage:Path"]!, "MessagesDbFiles");
                if (!Directory.Exists(dbPath)) Directory.CreateDirectory(dbPath);
                return new PartitionedObjectBucket<MessageInDatabaseEntity, int>("kahla", dbPath);
            });
            
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
            
            // Repositories
            services.AddScoped<UserOthersViewRepo>();
            services.AddScoped<UserInThreadViewRepo>();
            services.AddScoped<ThreadOthersViewRepo>();
            services.AddScoped<ThreadJoinedViewRepo>();
            
            // App services
            services.AddScoped<UserOthersViewAppService>();
            services.AddScoped<UserInThreadViewAppService>();
            services.AddScoped<ThreadOthersViewAppService>();
            services.AddScoped<ThreadJoinedViewAppService>();
            
            // Services
            services.AddScoped<WebPushClient>();
            services.AddScoped<WebPushService>();
            services.AddScoped<WebSocketPushService>();
            services.AddScoped<KahlaPushService>();
            services.AddScoped<OnlineJudger>();
            services.AddTaskCanon();

            // Controllers and web
            services
                .AddControllers()
                .AddApplicationPart(typeof(Startup).Assembly)
                .AddAiurProtocol();

            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
            });
        }

        public void Configure(WebApplication app)
        {
            app.UseWebSockets();
            app.UseMiddleware<HandleKahlaOptionsMiddleware>();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<OnlineDetectorMiddleware>();
            app.UseResponseCompression();
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
