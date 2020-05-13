using Aiursoft.Archon.SDK.Services;
using Aiursoft.Pylon;
using Aiursoft.SDK;
using EFCoreSecondLevelCacheInterceptor;
using Kahla.SDK.Models;
using Kahla.Server.Data;
using Kahla.Server.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using WebPush;

namespace Kahla.Server
{
    public class Startup
    {
        private IConfiguration _configuration { get; }
        private const string _cacheProviderName = "redis";

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
            AppsContainer.CurrentAppId = configuration["KahlaAppId"];
            AppsContainer.CurrentAppSecret = configuration["KahlaAppSecret"];
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var useRedis = _configuration["easycaching:enabled"] == true.ToString();

            services.AddDbContextPool<KahlaDbContext>((serviceProvider, optionsBuilder) =>
                optionsBuilder.UseSqlServer(_configuration.GetConnectionString("DatabaseConnection"))
                    .AddInterceptors(serviceProvider.GetRequiredService<SecondLevelCacheInterceptor>()));
            services.AddEFSecondLevelCache(options =>
            {
                if (useRedis)
                {
                    options.UseEasyCachingCoreProvider(_cacheProviderName).DisableLogging(true);
                }
                else
                {
                    options.UseMemoryCacheProvider().DisableLogging(true);
                }
                options.CacheAllQueries(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(30));
            });
            if (useRedis)
            {
                services.AddEasyCaching(option =>
                {
                    option.UseRedis(_configuration, _cacheProviderName);
                });
            }

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
