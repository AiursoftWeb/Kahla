using Aiursoft.Kahla.SDK.Models;

namespace Aiursoft.Kahla.SDK.Events;

public class NewFriendRequestEvent : KahlaEvent
{
    public NewFriendRequestEvent()
    {
        Type = EventType.NewFriendRequestEvent;
    }

    public required Request Request { get; set; }
}