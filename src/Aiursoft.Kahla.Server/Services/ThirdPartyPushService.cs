using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.Server.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WebPush;

namespace Aiursoft.Kahla.Server.Services;

public class ThirdPartyPushService(
    IConfiguration configuration,
    WebPushClient webPushClient,
    ILogger<ThirdPartyPushService> logger,
    KahlaDbContext dbContext)
{
    private readonly ILogger _logger = logger;

    public async Task PushAsync(Device device, object payload, string triggerEmail = "postermaster@aiursoft.com")
    {
        string vapidPublicKey = configuration.GetSection("VapidKeys")["PublicKey"]!;
        string vapidPrivateKey = configuration.GetSection("VapidKeys")["PrivateKey"]!;
        try
        {
            var pushSubscription = new PushSubscription(device.PushEndpoint, device.PushP256Dh, device.PushAuth);
            var vapidDetails = new VapidDetails("mailto:" + triggerEmail, vapidPublicKey, vapidPrivateKey);
            var payloadToken = JsonConvert.SerializeObject(payload, new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            });
            await webPushClient.SendNotificationAsync(pushSubscription, payloadToken, vapidDetails);
        }
        catch (WebPushException e)
        {
            dbContext.Devices.Remove(device);
            await dbContext.SaveChangesAsync();
            _logger.LogCritical(e, "An WebPush error occured while calling WebPush API: {EMessage} on device: {DeviceId}", e.Message, device.Id);
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "An error occured while calling WebPush API: {EMessage} on device: {DeviceId}", e.Message, device.Id);
        }
    }
}