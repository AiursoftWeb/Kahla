using Aiursoft.Kahla.SDK.Models;

namespace Aiursoft.Kahla.SDK.Events;

public class NewMemberEvent : KahlaEvent
{
    public NewMemberEvent()
    {
        Type = EventType.NewMemberEvent;
    }
    public required int ConversationId { get; set; }
    public required KahlaUser NewMember { get; set; }
}