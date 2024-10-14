using Aiursoft.AiurProtocol.Models;
using Aiursoft.Kahla.SDK.Models.Mapped;

namespace Aiursoft.Kahla.SDK.Models.ViewModels;

public class ThreadDetailsViewModel : AiurResponse
{
    public required KahlaThreadMappedJoinedView? Thread { get; set; }
}