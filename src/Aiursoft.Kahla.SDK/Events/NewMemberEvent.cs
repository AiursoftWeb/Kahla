using Aiursoft.Kahla.SDK.Models.Mapped;

namespace Aiursoft.Kahla.SDK.Events;

public class NewMemberEvent : KahlaEvent
{
    public NewMemberEvent()
    {
        Type = EventType.NewMemberEvent;
    }
    public required int ConversationId { get; set; }
    public required KahlaUserMappedPublicView NewMember { get; set; }
}