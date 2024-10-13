using Aiursoft.Kahla.SDK.Models.Entities;

namespace Aiursoft.Kahla.SDK.Events;

public class NewMessageEvent : KahlaEvent
{
    public NewMessageEvent()
    {
        Type = EventType.NewMessage;
    }

    public bool Muted { get; set; }
    public int ThreadId => Message.ThreadId;
    public required Message Message { get; init; }
    public bool Mentioned { get; set; }
}