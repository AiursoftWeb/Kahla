using Aiursoft.AiurObserver;
using Aiursoft.AiurObserver.WebSocket;
using Aiursoft.Kahla.Server.Models;

namespace Aiursoft.Kahla.Server.Controllers;

public class ThreadReflectConsumer(
    ThreadStatusInMemoryCache threadStatusCache,
    string listeningUserId,
    ILogger<MessagesController> logger,
    ObservableWebSocket socket)
    : IConsumer<MessageInDatabaseEntity[]>
{
    public async Task Consume(MessageInDatabaseEntity[] newEntities)
    {
        // Ensure the user is in the thread.
        if (!threadStatusCache.IsUserInThread(listeningUserId))
        {
            logger.LogWarning("User with ID: {UserId} is trying to listen to a thread that he is not in. Rejected.", listeningUserId);
            return;
        }
        
        // Send to the client.
        logger.LogInformation("Reflecting {Count} new messages to the client with Id: '{ClientId}'.", newEntities.Length, listeningUserId);
        await socket.Send(SDK.Extensions.Serialize(newEntities.Select(t => t.ToCommit())));

        // Clear current user's unread message count.
        threadStatusCache.ClearUserUnReadAmount(listeningUserId);
        
        // Clear current user's at status.
        threadStatusCache.ClearAtForUser(listeningUserId);
    }
}