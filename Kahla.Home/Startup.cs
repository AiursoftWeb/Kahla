using Aiursoft.Archon.SDK;
using Aiursoft.Archon.SDK.Services;
using Aiursoft.Gateway.SDK;
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
            AppsContainer.CurrentAppId = configuration["KahlaHomeAppId"];
            AppsContainer.CurrentAppSecret = configuration["KahlaHomeAppSecret"];
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAiurMvc();
            services.AddArchonServer();
            services.AddStargateServer();
            services.AddProbeServer();
            services.AddGatewayServer();
            services.AddObserverServer();
            services.AddBasic();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseAiurUserHandler(env.IsDevelopment());
            app.UseAiursoftDefault();
        }
    }
}
