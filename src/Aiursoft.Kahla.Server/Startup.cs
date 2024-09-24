﻿using Aiursoft.AiurProtocol.Server;
using Aiursoft.DbTools.Sqlite;
using Aiursoft.DocGenerator.Services;
using Aiursoft.Kahla.SDK.ModelsOBS;
using Aiursoft.Kahla.Server.Data;
using Aiursoft.Kahla.Server.Middlewares;
using Aiursoft.WebTools.Abstractions.Models;
using Microsoft.AspNetCore.Authorization;
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
            app.UseAiursoftDocGenerator(options =>
            {
                options.DocAddress = "/api/doc";
                options.Format = DocFormat.Markdown;
                options.RequiresAuthorized = (action, controller) =>
                {
                    return
                        action.CustomAttributes.Any(t => t.AttributeType == typeof(AuthorizeAttribute)) ||
                        controller.CustomAttributes.Any(t => t.AttributeType == typeof(AuthorizeAttribute));
                };
            });

        }
    }
}
