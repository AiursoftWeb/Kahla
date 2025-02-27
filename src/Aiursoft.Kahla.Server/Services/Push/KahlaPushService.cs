using Aiursoft.Canon;
using Aiursoft.Kahla.Entities;
using Aiursoft.Kahla.SDK.Events.Abstractions;
using Aiursoft.Kahla.Server.Data;
using Aiursoft.Kahla.Server.Services.Push.WebPush;
using Aiursoft.Kahla.Server.Services.Push.WebSocket;

namespace Aiursoft.Kahla.Server.Services.Push;

public class KahlaPushService(
    DevicesCache devicesCache,
    ILogger<KahlaPushService> logger,
    KahlaRelationalDbContext context,
    CanonPool canonPool,
    WebSocketPushService wsPusher,
    WebPushService webPusher)
{
    public async Task PushToUser(string userId, KahlaEvent payload, PushMode mode = PushMode.AllPath)
    {
        // WebSocket push.
        if (mode is PushMode.AllPath or PushMode.OnlyWebSocket)
        {
            logger.LogInformation("Pushing to user: {UserId} with WebSocket...", userId);
            canonPool.RegisterNewTaskToPool(async () => { await wsPusher.PushAsync(userId, payload); });
        }

        // Web push.
        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (mode is PushMode.AllPath or PushMode.OnlyWebPush)
        {
            // Load his devices
            var hisDevices = await devicesCache.GetValidDevicesWithCache(userId);
            logger.LogInformation("Pushing to user: {UserId} with {DeviceCount} WebPush devices...", userId,
                hisDevices.Count);
            foreach (var hisDevice in hisDevices)
            {
                canonPool.RegisterNewTaskToPool(async () => { await webPusher.PushAsync(hisDevice, payload); });
            }
        }

        // Dry run.
        if (mode is PushMode.DryRun)
        {
            logger.LogWarning("Dry run mode is enabled. No push will be sent.");
        }

        // Do actual push.
        await canonPool.RunAllTasksInPoolAsync(Extensions.GetLimitedNumber(
            min: 8,
            max: 32,
            suggested: Environment.ProcessorCount));
        
        // Some devices may be invalid, remove them.
        await context.SaveChangesAsync();
    }
}