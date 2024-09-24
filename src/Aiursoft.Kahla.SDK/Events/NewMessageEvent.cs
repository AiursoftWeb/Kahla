using Aiursoft.Kahla.SDK.ModelsOBS;

namespace Aiursoft.Kahla.SDK.Events;

public class NewMessageEvent : KahlaEvent
{
    public NewMessageEvent(Message message, bool muted = false)
    {
        Type = EventType.NewMessage;
        Message = message;
        Muted = muted;
    }

    public bool Muted { get; set; }
    public int ConversationId => Message.ConversationId;
    public Message Message { get; set; }
}