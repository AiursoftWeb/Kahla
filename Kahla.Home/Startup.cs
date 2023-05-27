﻿using Aiursoft.Directory.SDK;
using Aiursoft.Observer.SDK;
using Aiursoft.Probe.SDK;
using Aiursoft.SDK;
using Aiursoft.Stargate.SDK;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Kahla.Home
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAiurMvc();
            services.AddStargateServer(Configuration.GetConnectionString("StargateConnection"));

            services.AddAiursoftProbe(Configuration.GetSection("AiursoftProbe")); // For file storaging.
            services.AddAiursoftObserver(Configuration.GetSection("AiursoftObserver")); // For error reporting.
            services.AddAiursoftAuthentication(Configuration.GetSection("AiursoftAuthentication")); // For authentication.
            services.AddAiursoftSDK();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseAiurUserHandler(env.IsDevelopment());
            app.UseAiursoftDefault();
        }
    }
}
