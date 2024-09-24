using Aiursoft.Kahla.SDK.ModelsOBS;

namespace Aiursoft.Kahla.SDK.Events;

public class FriendDeletedEvent : KahlaEvent
{
    public FriendDeletedEvent()
    {
        Type = EventType.FriendDeletedEvent;
    }

    public required  KahlaUser Trigger { get; set; }
    public required int ConversationId { get; init; }
}