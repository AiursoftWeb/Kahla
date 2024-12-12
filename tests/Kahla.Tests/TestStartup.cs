using Aiursoft.Kahla.Server;
using Aiursoft.Kahla.Server.Services.Push.WebPush;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Aiursoft.Kahla.Tests;

public class TestStartup : Startup
{
    public override void ConfigureServices(IConfiguration configuration, IWebHostEnvironment environment, IServiceCollection services)
    {
        base.ConfigureServices(configuration, environment, services);
        services.RemoveAll<WebPushService>();
        services.AddScoped<WebPushService, MockWebPushService>();
    }
}