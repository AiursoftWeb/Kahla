using Aiursoft.AiurProtocol.Models;

namespace Aiursoft.Kahla.SDK.Models.ApiViewModels;

public class MeViewModel : AiurResponse
{
    public required KahlaUser User { get; init; }
}