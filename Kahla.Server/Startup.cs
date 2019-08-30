using Aiursoft.Pylon;
using Aiursoft.Pylon.Services;
using Aiursoft.Pylon.Services.ToAPIServer;
using Aiursoft.Pylon.Services.ToOSSServer;
using Aiursoft.Pylon.Services.ToStargateServer;
using Kahla.Server.Data;
using Kahla.Server.Middlewares;
using Kahla.Server.Models;
using Kahla.Server.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using WebPush;

namespace Kahla.Server
{
    public class Startup
    {
        private IConfiguration Configuration { get; }
        private SameSiteMode Mode { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Mode = Convert.ToBoolean(configuration["LaxCookie"]) ? SameSiteMode.Lax : SameSiteMode.None;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };
            services.ConfigureLargeFileUpload();
            services.AddApplicationInsightsTelemetry();
            services.AddDbContext<KahlaDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DatabaseConnection")));

            services.AddIdentity<KahlaUser, IdentityRole>()
                .AddEntityFrameworkStores<KahlaDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<List<DomainSettings>>(Configuration.GetSection("AppDomain"));

            services.ConfigureApplicationCookie(t => t.Cookie.SameSite = Mode);

            services.AddMemoryCache();

            services.AddMvc().AddJsonOptions(opt =>
            {
                opt.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                opt.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });
            services.AddAiursoftAuth<KahlaUser>();

            services.AddSingleton<IHostedService, TimedCleaner>();
            services.AddSingleton<IHostedService, EmailNotifier>();
            services.AddScoped<UserService>();
            services.AddScoped<SecretService>();
            services.AddScoped<VersionChecker>();
            services.AddScoped<OwnerChecker>();
            // Web Push Service
            services.AddScoped<WebPushClient>();
            services.AddScoped<ThirdPartyPushService>();
            // Stargate Push Service
            services.AddScoped<ChannelService>();
            services.AddScoped<PushMessageService>();
            // Final Kahla Push Service
            services.AddScoped<KahlaPushService>();
            services.AddTransient<AiurEmailSender>();
        }

        public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseHandleRobots();
                app.UseEnforceHttps();
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseAiursoftAuthenticationFromConfiguration(Configuration, "Kahla");
            app.UseMiddleware<HandleKahlaOptionsMiddleware>();
            app.UseAuthentication();
            app.UseLanguageSwitcher();
            app.UseMvcWithDefaultRoute();
            app.UseDocGenerator();
        }
    }
}
