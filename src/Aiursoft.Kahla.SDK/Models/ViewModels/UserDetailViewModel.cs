using Aiursoft.AiurProtocol.Models;
using Aiursoft.Kahla.SDK.Models.Mapped;

namespace Aiursoft.Kahla.SDK.Models.ViewModels;

public class UserDetailViewModel : AiurResponse
{
    public required KahlaUserMappedDetailedOthersView User { get; init; }
}