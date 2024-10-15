using Aiursoft.AiurProtocol.Models;
using Aiursoft.Kahla.SDK.Models.Mapped;

namespace Aiursoft.Kahla.SDK.Models.ViewModels;

public class MyContactsViewModel : AiurResponse
{
    public required List<KahlaUserMappedOthersView> KnownContacts { get; init; } = new();
    public required int TotalKnownContacts { get; init; }
}