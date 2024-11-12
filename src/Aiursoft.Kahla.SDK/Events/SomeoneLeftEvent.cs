
using Aiursoft.Kahla.SDK.Models.Mapped;

namespace Aiursoft.Kahla.SDK.Events;

public class SomeoneLeftEvent : KahlaEvent
{
    public SomeoneLeftEvent()
    {
        Type = EventType.SomeoneLeftEvent;
    }
    public required int ConversationId { get; set; }
    public required KahlaUserMappedPublicView LeftUser { get; set; }
}