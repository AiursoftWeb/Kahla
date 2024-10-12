using Aiursoft.AiurProtocol.Models;
using Aiursoft.Kahla.SDK.Models.Mapped;

namespace Aiursoft.Kahla.SDK.Models.ViewModels;

public class MyBlocksViewModel : AiurResponse
{
    public required List<KahlaUserMappedOthersView> KnownBlocks { get; set; } = new();
}