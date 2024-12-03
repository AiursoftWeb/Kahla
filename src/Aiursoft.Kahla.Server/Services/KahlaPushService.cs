using Aiursoft.Canon;
using Aiursoft.Kahla.SDK.Events;
using Aiursoft.Kahla.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Services;

public enum PushMode
{
    AllPath,
    OnlyWebPush,
    OnlyWebSocket,
    DryRun
}

public class BufferedKahlaPushService(
    ILogger<BufferedKahlaPushService> logger,
    QuickMessageAccess quickMessageAccess,
    CanonQueue canonQueue)
{
    private void QueuePushEventsToUser(string userId, PushMode mode, IEnumerable<KahlaEvent> payloads)
    {
        canonQueue.QueueWithDependency<KahlaPushService>(async p =>
        {
            foreach (var payload in payloads)
            {
                await p.PushToUser(userId, payload, mode);
            }
        });
    }

    public void QueuePushEventsToUsersInThread(int threadId, PushMode mode, KahlaEvent payload)
    {
        var usersInThread = quickMessageAccess.GetUsersInThread(threadId);
        foreach (var user in usersInThread)
        {
            QueuePushEventsToUser(user, mode, [payload]);
        }
    }

    public void QueuePushEventToUser(string userId, PushMode mode, KahlaEvent payload) =>
        QueuePushEventsToUser(userId, mode, [payload]);
}

public class KahlaPushService(
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
        // TODO: Refactor this to in memory cache to improve performance.
        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (mode is PushMode.AllPath or PushMode.OnlyWebPush)
        {
            // Load his devices
            var hisDevices = await context
                .Devices
                .AsNoTracking()
                .Where(t => t.OwnerId == userId)
                .ToListAsync();
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