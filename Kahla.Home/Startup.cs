using Aiursoft.Canon;
using Aiursoft.Directory.SDK;
using Aiursoft.Observer.SDK;
using Aiursoft.Probe.SDK;
using Aiursoft.SDK;
using Aiursoft.WebTools.Models;

namespace Kahla.Home
{
    public class Startup : IWebStartup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IConfiguration configuration, IWebHostEnvironment environment, IServiceCollection services)
        {
            services.AddAiursoftWebFeatures();
            services.AddTaskCanon();
            services.AddAiursoftProbe(Configuration.GetSection("AiursoftProbe")); // For file storage.
            services.AddAiursoftObserver(Configuration.GetSection("AiursoftObserver")); // For error reporting.
            services.AddAiursoftAppAuthentication(Configuration.GetSection("AiursoftAuthentication")); // For authentication.
            services.AddScannedServices();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseAiursoftHandler(env.IsDevelopment());
            app.UseAiursoftAppRouters();
        }
    }
}
