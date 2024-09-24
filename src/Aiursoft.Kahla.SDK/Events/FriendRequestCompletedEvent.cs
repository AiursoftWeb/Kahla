using Aiursoft.Kahla.SDK.ModelsOBS;

namespace Aiursoft.Kahla.SDK.Events;

public class FriendRequestCompletedEvent : KahlaEvent
{
    public FriendRequestCompletedEvent()
    {
        Type = EventType.FriendRequestCompletedEvent;
    }
    public required Request Request { get; set; }
    public required bool Result { get; set; }
    public required PrivateConversation CreatedConversation { get; set; }
}