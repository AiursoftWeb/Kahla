using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Models.Entities;

namespace Aiursoft.Kahla.SDK.Events;

public class SomeoneLeftEvent : KahlaEvent
{
    public SomeoneLeftEvent()
    {
        Type = EventType.SomeoneLeftEvent;
    }
    public required int ConversationId { get; set; }
    public required KahlaUser LeftUser { get; set; }
}