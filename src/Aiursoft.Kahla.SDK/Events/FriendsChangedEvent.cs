using Aiursoft.Kahla.SDK.ModelsOBS;

namespace Aiursoft.Kahla.SDK.Events;

public class FriendsChangedEvent : KahlaEvent
{
    public FriendsChangedEvent()
    {
        Type = EventType.FriendsChangedEvent;
    }
    public required Request Request { get; set; }
    public required bool Result { get; set; }
    public required PrivateConversation CreatedConversation { get; set; }
}