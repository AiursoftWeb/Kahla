namespace Aiursoft.Kahla.SDK.Events;

public class TimerUpdatedEvent : KahlaEvent
{
    public TimerUpdatedEvent()
    {
        Type = EventType.TimerUpdatedEvent;
    }
    public int ConversationId { get; set; }
    public int NewTimer { get; set; }
}