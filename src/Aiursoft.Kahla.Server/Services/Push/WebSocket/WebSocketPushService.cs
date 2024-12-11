using Aiursoft.Kahla.SDK.Events.Abstractions;
using Aiursoft.Kahla.Server.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WebPush;

namespace Aiursoft.Kahla.Server.Services.Push.WebSocket;

public class WebSocketPushService(
    ILogger<WebSocketPushService> logger,
    ChannelsInMemoryDb context)
{
    public async Task PushAsync(string userId, KahlaEvent payload)
    {
        var channel = context.GetUserChannel(userId);
        try
        {
            logger.LogInformation("Pushing a WebSocket message to user: {Id} with WebSocket.", userId); 
            var payloadToken = JsonConvert.SerializeObject(payload, new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            });
            await channel.BroadcastAsync(payloadToken);
            logger.LogInformation("Successfully pushed a WebSocket message to user: {Id}.", userId);
        }
        catch (WebPushException e)
        {
            logger.LogCritical(e, "A WebSocket error occured while calling WebSocket API: {EMessage} on user: {Id}", e.Message, userId);
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "An unknown error occured while calling WebSocket API: {EMessage} on user: {Id}", e.Message, userId);
        }
    }
}