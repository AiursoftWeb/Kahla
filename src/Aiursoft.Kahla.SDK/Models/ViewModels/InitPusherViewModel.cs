using Aiursoft.AiurProtocol.Models;

namespace Aiursoft.Kahla.SDK.Models.ViewModels;

public class InitPusherViewModel : AiurResponse
{
    public required string WebSocketEndpoint { get; init; }
}