using Aiursoft.AiurProtocol.Models;

namespace Aiursoft.Kahla.SDK.Models.ViewModels;

public class MeViewModel : AiurResponse
{
    public required KahlaUser User { get; init; }
}

public class InitPusherViewModel : AiurResponse
{
    public required string Otp { get; init; }
    public required DateTime OtpValidTo { get; init; }
    public required string WebSocketEndpoint { get; init; }
}