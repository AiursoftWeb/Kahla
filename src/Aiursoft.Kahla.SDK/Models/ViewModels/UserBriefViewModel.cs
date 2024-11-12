using Aiursoft.AiurProtocol.Models;
using Aiursoft.Kahla.SDK.Models.Mapped;

namespace Aiursoft.Kahla.SDK.Models.ViewModels;

public class UserBriefViewModel : AiurResponse
{
    public required KahlaUserMappedPublicView BriefUser { get; init; }
}