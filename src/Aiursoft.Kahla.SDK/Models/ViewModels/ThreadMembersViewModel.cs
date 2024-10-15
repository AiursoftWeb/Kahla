using Aiursoft.AiurProtocol.Models;
using Aiursoft.Kahla.SDK.Models.Mapped;

namespace Aiursoft.Kahla.SDK.Models.ViewModels;

public class ThreadMembersViewModel : AiurResponse
{
    public required List<KahlaUserMappedInThreadView> Members { get; init; }
    public required int TotalCount { get; init; }
}