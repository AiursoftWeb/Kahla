using Aiursoft.Kahla.SDK.ModelsOBS;

namespace Aiursoft.Kahla.SDK.Events;

public class NewFriendRequestEvent : KahlaEvent
{
    public NewFriendRequestEvent()
    {
        Type = EventType.NewFriendRequestEvent;
    }

    public required Request Request { get; set; }
}