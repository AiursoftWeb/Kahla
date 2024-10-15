using Aiursoft.AiurProtocol.Models;

namespace Aiursoft.Kahla.SDK.Models.ViewModels;

public class CreateNewThreadViewModel : AiurResponse
{
    public required int NewThreadId { get; init; }
}