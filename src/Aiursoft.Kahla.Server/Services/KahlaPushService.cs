using Aiursoft.Canon;
using Aiursoft.Kahla.SDK.Events;
using Aiursoft.Kahla.Server.Data;
using Aiursoft.Kahla.Server.Models.Entities;

namespace Aiursoft.Kahla.Server.Services;

public enum PushMode
{
    AllPath,
    OnlyWebPush,
    OnlyWebSocket,
    DryRun
}

public class KahlaPushService(
    ILogger<KahlaPushService> logger,
    KahlaRelationalDbContext context,
    CanonPool canonPool,
    WebSocketPushService wsPusher,
    WebPushService webPusher)
{
    public async Task PushToUser(KahlaUser user, KahlaEvent payload, PushMode mode = PushMode.AllPath)
    {
        // WebSocket push.
        if (mode is PushMode.AllPath or PushMode.OnlyWebSocket)
        {
            logger.LogInformation("Pushing to user: {UserId} with WebSocket...", user.Id);
            canonPool.RegisterNewTaskToPool(async () => { await wsPusher.PushAsync(user.Id, payload); });
        }
        
        // Web push.
        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (mode is PushMode.AllPath or PushMode.OnlyWebPush)
        {
            // Load his devices if not loaded.
            if (!context.Entry(user).Collection(t => t.HisDevices).IsLoaded)
            {
                logger.LogInformation("Loading devices for user: {UserId}, since devices were not loaded...", user.Id);
                await context.Entry(user).Collection(t => t.HisDevices).LoadAsync();
            }
            logger.LogInformation("Pushing to user: {UserId} with {DeviceCount} WebPush devices...", user.Id, user.HisDevices.Count());
            foreach (var hisDevice in user.HisDevices)
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
    }

    // public void NewMemberEvent(KahlaUser receiver, KahlaUser newMember, int conversationId)
    // {
    //     var newMemberEvent = new NewMemberEvent
    //     {
    //         NewMember = newMember,
    //         ConversationId = conversationId
    //     };
    //     _canon.FireAsync<WebSocketPushService>(s => s.PushAsync(receiver, newMemberEvent));
    // }
    //
    // public void SomeoneLeftEvent(KahlaUser receiver, KahlaUser leftMember, int conversationId)
    // {
    //     var someoneLeftEvent = new SomeoneLeftEvent
    //     {
    //         LeftUser = leftMember,
    //         ConversationId = conversationId
    //     };
    //     _canon.FireAsync<WebSocketPushService>(s => s.PushAsync(receiver, someoneLeftEvent));
    // }
    //
    // public void DissolveEvent(KahlaUser receiver, int conversationId)
    // {
    //     var dissolveEvent = new DissolveEvent
    //     {
    //         ConversationId = conversationId
    //     };
    //     _canon.FireAsync<WebSocketPushService>(s => s.PushAsync(receiver, dissolveEvent));
    // }
}
