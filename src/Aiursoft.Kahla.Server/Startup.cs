using Aiursoft.AiurProtocol.Server;
using Aiursoft.ArrayDb.Partitions;
using Aiursoft.Canon;
using Aiursoft.DocGenerator.Services;
using Aiursoft.InMemoryKvDb;
using Aiursoft.Kahla.Server.Attributes;
using Aiursoft.Kahla.Server.Data;
using Aiursoft.Kahla.Server.Middlewares;
using Aiursoft.Kahla.Server.Models.Entities;
using Aiursoft.Kahla.Server.Services;
using Aiursoft.Kahla.Server.Services.AppService;
using Aiursoft.Kahla.Server.Services.BackgroundJobs;
using Aiursoft.Kahla.Server.Services.Messages;
using Aiursoft.Kahla.Server.Services.Push;
using Aiursoft.Kahla.Server.Services.Push.WebPush;
using Aiursoft.Kahla.Server.Services.Push.WebSocket;
using Aiursoft.Kahla.Server.Services.Repositories;
using Aiursoft.Kahla.Server.Services.Storage;
using Aiursoft.Kahla.Server.Services.Storage.ImageProcessing;
using Aiursoft.WebTools.Abstractions.Models;
using Microsoft.AspNetCore.Identity;
using WebPush;

namespace Aiursoft.Kahla.Server
{
    public class Startup : IWebStartup
    {
        public virtual void ConfigureServices(IConfiguration configuration, IWebHostEnvironment environment, IServiceCollection services)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            // Background jobs
            services.AddSingleton<IHostedService, UnreadPersistsService>();
            
            // In Memory Databases.
            services.AddMemoryCache();
            services.AddSingleton<ChannelsInMemoryDb>();
            services.AddSingleton<LocksInMemoryDb>();
            services.AddNamedLruMemoryStore<SemaphoreSlim, string>(
                onNotFound: _ => new SemaphoreSlim(1, 1),
                maxCachedItemsCount: 0x1000); // 4096
            services.AddNamedLruMemoryStore<ReaderWriterLockSlim, int>(
                onNotFound: _ => new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion),
                maxCachedItemsCount: 0x8000); // 32768
            
            // Memory acceleration layers
            services.AddSingleton<QuickMessageAccess>();
            
            // Relational Database
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
                .AddEntityFrameworkStores<KahlaRelationalDbContext>()
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
            
            // Push services
            services.AddTaskCanon();
            services.AddScoped<DevicesCache>();
            services.AddScoped<WebPushClient>();
            services.AddScoped<WebPushService>();
            services.AddScoped<WebSocketPushService>();
            services.AddScoped<KahlaPushService>();
            services.AddScoped<BufferedKahlaPushService>();
            
            // Message services
            services.AddScoped<ChannelMessageService>();
            
            // Storage services.
            services.AddScoped<PathResolver>();
            services.AddScoped<StorageService>();
            services.AddSingleton<FileLockProvider>(); 
            services.AddScoped<IImageProcessingService, ImageProcessingService>();
            
            // Online detector
            services.AddScoped<OnlineDetector>();

            // Controllers and web
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
