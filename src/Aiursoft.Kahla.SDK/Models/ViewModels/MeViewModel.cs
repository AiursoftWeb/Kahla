using Aiursoft.AiurProtocol.Models;
using Aiursoft.Kahla.SDK.Models.Mapped;

namespace Aiursoft.Kahla.SDK.Models.ViewModels;

public class MeViewModel : AiurResponse
{
    public required KahlaUserMappedPublicView User { get; init; }
    
    public required PrivateSettings PrivateSettings { get; init; }
}