namespace Aiursoft.Kahla.SDK.Events;

public class DissolveEvent : KahlaEvent
{
    public DissolveEvent()
    {
        Type = EventType.DissolveEvent;
    }

    public int ConversationId { get; set; }
}