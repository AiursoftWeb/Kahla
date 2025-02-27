using Aiursoft.AiurObserver;
using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.SDK.Services;
using Aiursoft.Kahla.Server.Services.Messages;

namespace Aiursoft.Kahla.Server.Controllers;

public class ClientPushConsumer(
    ChannelMessageService channelMessageService,
    KahlaUserMappedPublicView userView,
    int threadId,
    ILogger<MessagesController> logger)
    : IConsumer<string>
{
    public async Task Consume(string clientPushed)
    {
        if (clientPushed.Length > 0xFFFF)
        {
            logger.LogWarning("User with ID: {UserId} is trying to push a message that is too large. Rejected. Max allowed size is 65535. He pushed {Size} bytes.", userView.Id, clientPushed.Length);
            return;
        }
        var model = SDK.Extensions.Deserialize<List<Commit<ChatMessage>>>(clientPushed);
        await channelMessageService.SendMessagesToChannel(model, threadId, userView);
    }
}