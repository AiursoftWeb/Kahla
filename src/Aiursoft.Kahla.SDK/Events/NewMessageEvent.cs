using Aiursoft.Kahla.SDK.ModelsOBS;

namespace Aiursoft.Kahla.SDK.Events;

public class NewMessageEvent : KahlaEvent
{
    public NewMessageEvent(Message message)
    {
        Type = EventType.NewMessage;
        Message = message;
    }
    
    public bool Muted { get; set; }
        
    public int ConversationId => Message.ConversationId;
    public Message Message { get; set; }
    public string? PreviousMessageId { get; set; }
}