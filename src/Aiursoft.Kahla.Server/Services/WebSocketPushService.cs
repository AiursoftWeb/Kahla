using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.Server.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WebPush;

namespace Aiursoft.Kahla.Server.Services;

public class WebSocketPushService(
    ILogger<WebSocketPushService> logger,
    InMemoryDataContext context)
{
    public async Task PushAsync(KahlaUser user, object payload)
    {
        var channel = context.GetMyChannel(user.Id);
        try
        {
            logger.LogInformation("Trying to push a message to user: {Email}.", user.Email); 
            var payloadToken = JsonConvert.SerializeObject(payload, new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            });
            await channel.BroadcastAsync(payloadToken);
            logger.LogInformation("Successfully pushed a WebSocket message to user: {Email}.", user.Email);
        }
        catch (WebPushException e)
        {
            logger.LogCritical(e, "A WebSocket error occured while calling WebSocket API: {EMessage} on user: {Email}", e.Message, user.Email);
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "An unknown error occured while calling WebSocket API: {EMessage} on user: {Email}", e.Message, user.Email);
        }
    }
}