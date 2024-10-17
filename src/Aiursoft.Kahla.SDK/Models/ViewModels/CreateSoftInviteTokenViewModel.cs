using Aiursoft.AiurProtocol.Models;

namespace Aiursoft.Kahla.SDK.Models.ViewModels;

public class CreateSoftInviteTokenViewModel : AiurResponse
{
    public required string Token { get; set; }
}