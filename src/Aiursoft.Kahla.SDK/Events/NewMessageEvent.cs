using Aiursoft.Kahla.SDK.ModelsOBS;

namespace Aiursoft.Kahla.SDK.Events;

public class NewMessageEvent : KahlaEvent
{
    public NewMessageEvent()
    {
        Type = EventType.NewMessage;
    }
    public bool Muted { get; set; }
        
    public int ConversationId => Message.ConversationId;
    public required Message Message { get; set; }
    public required string PreviousMessageId { get; set; }
}