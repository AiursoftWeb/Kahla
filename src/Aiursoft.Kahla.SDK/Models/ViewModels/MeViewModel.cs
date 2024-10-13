using Aiursoft.AiurProtocol.Models;
using Aiursoft.Kahla.SDK.Models.Entities;

namespace Aiursoft.Kahla.SDK.Models.ViewModels;

public class MeViewModel : AiurResponse
{
    public required KahlaUser User { get; init; }
    
    public required PrivateSettings PrivateSettings { get; init; }
}