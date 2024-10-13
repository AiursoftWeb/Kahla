namespace Aiursoft.Kahla.SDK.Models.ViewModels;

public class PrivateSettings
{
    public required int ThemeId { get; init; }
    public required bool AllowHardInvitation { get; init; }
    public required bool EnableEmailNotification { get; init; }
    public required bool AllowSearchByName { get; init; }
    public required bool EnableEnterToSendMessage { get; init; }
    public required bool EnableHideMyOnlineStatus { get; init; }
}