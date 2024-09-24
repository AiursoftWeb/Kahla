using Aiursoft.AiurProtocol.Models;

namespace Aiursoft.Kahla.SDK.Models.ViewModels;

public class MeViewModel : AiurResponse
{
    public required KahlaUser User { get; init; }
    
    public int ThemeId => User.ThemeId;

    public bool EnableEmailNotification => User.EnableEmailNotification;

    public bool ListInSearchResult => User.ListInSearchResult;
    
    public bool EnableEnterToSendMessage => User.EnableEnterToSendMessage;
    
    public bool EnableHideMyOnlineStatus => User.EnableHideMyOnlineStatus;
}