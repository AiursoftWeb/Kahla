using Aiursoft.Kahla.SDK.Events;
using Aiursoft.Kahla.Server.Data;
using Aiursoft.Kahla.Server.Models.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WebPush;

namespace Aiursoft.Kahla.Server.Services;

public class WebPushService(
    DevicesCache cache,
    IConfiguration configuration,
    WebPushClient webPushClient,
    ILogger<WebPushService> logger,
    KahlaRelationalDbContext relationalDbContext)
{
    public async Task PushAsync(Device device, KahlaEvent payload, string triggerEmail = "postermaster@aiursoft.com")
    {
        var vapidPublicKey = configuration.GetSection("VapidKeys")["PublicKey"]!;
        var vapidPrivateKey = configuration.GetSection("VapidKeys")["PrivateKey"]!;
        try
        {
            logger.LogInformation(
                "Trying to push a message to device: {DeviceId}. Device endpoint: {Endpoint}, Device P256DH: {P256DH}, Device Auth: {Auth}",
                device.Id, device.PushEndpoint, device.PushP256Dh, device.PushAuth);
            var pushSubscription = new PushSubscription(device.PushEndpoint, device.PushP256Dh, device.PushAuth);
            var vapidDetails = new VapidDetails("mailto:" + triggerEmail, vapidPublicKey, vapidPrivateKey);
            var payloadToken = JsonConvert.SerializeObject(payload, new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            });
            await webPushClient.SendNotificationAsync(pushSubscription, payloadToken, vapidDetails);
            logger.LogInformation("Successfully pushed a message to a WebPush device: {DeviceId}", device.Id);
        }
        catch (WebPushException e)
        {
            cache.ClearCacheForUser(device.OwnerId);
            relationalDbContext.Devices.Remove(device);
            logger.LogCritical(e,
                "A  WebPush error occured while calling WebPush API: {EMessage} on device: {DeviceId}", e.Message,
                device.Id);
        }
        catch (Exception e)
        {
            cache.ClearCacheForUser(device.OwnerId);
            relationalDbContext.Devices.Remove(device);
            logger.LogCritical(e,
                "An unknown error occured while calling WebPush API: {EMessage} on device: {DeviceId}", e.Message,
                device.Id);
        }
    }
}