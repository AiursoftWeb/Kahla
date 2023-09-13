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
        public void ConfigureServices(IConfiguration configuration, IWebHostEnvironment environment, IServiceCollection services)
        {
            services.AddAiursoftWebFeatures();
            services.AddTaskCanon();
            services.AddAiursoftProbe(configuration.GetSection("AiursoftProbe")); // For file storage.
            services.AddAiursoftObserver(configuration.GetSection("AiursoftObserver")); // For error reporting.
            services.AddAiursoftAppAuthentication(configuration.GetSection("AiursoftAuthentication")); // For authentication.
            services.AddScannedServices();
        }

        public void Configure(WebApplication app)
        {
            app.UseAiursoftHandler(app.Environment.IsDevelopment());
            app.UseAiursoftAppRouters();
        }
    }
}
