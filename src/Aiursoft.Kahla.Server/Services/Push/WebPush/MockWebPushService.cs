using Aiursoft.Kahla.Entities;
using Aiursoft.Kahla.Entities.Entities;
using Aiursoft.Kahla.SDK.Events.Abstractions;
using Aiursoft.Kahla.Server.Data;
using WebPush;

namespace Aiursoft.Kahla.Server.Services.Push.WebPush;

public class MockWebPushService(
    DevicesCache cache,
    IConfiguration configuration,
    WebPushClient webPushClient,
    ILogger<WebPushService> logger,
    KahlaRelationalDbContext relationalDbContext)
    : WebPushService(cache, configuration, webPushClient, logger,
        relationalDbContext)
{
    public static List<KahlaEvent?> PushedPayloads { get; private set; } = new List<KahlaEvent?>();
    
    public override Task PushAsync(
        Device device, 
        KahlaEvent payload,
        string triggerEmail = "postermaster@aiursoft.com")
    {
        PushedPayloads.Add(payload);
        return Task.CompletedTask;
    }
}