using Aiursoft.AiurProtocol.Models;

namespace Aiursoft.Kahla.SDK.Models.ViewModels;

public class MeViewModel : AiurResponse
{
    public required KahlaUser User { get; init; }
    
    public int ThemeId { get; init; }

    public bool EnableEmailNotification { get; init; }
    public bool ListInSearchResult { get; init; }

    public bool EnableEnterToSendMessage { get; init; }

    public bool EnableHideMyOnlineStatus { get; init; }
}