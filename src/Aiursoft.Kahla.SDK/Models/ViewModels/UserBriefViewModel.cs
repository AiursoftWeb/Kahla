using Aiursoft.AiurProtocol.Models;
using Aiursoft.Kahla.SDK.Models.Entities;

namespace Aiursoft.Kahla.SDK.Models.ViewModels;

public class UserBriefViewModel : AiurResponse
{
    public required KahlaUser BriefUser { get; init; }
}