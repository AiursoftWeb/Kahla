using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Models.Conversations;

namespace Aiursoft.Kahla.SDK.Events;

public class FriendRequestCompletedEvent : KahlaEvent
{
    public FriendRequestCompletedEvent()
    {
        Type = EventType.FriendRequestCompletedEvent;
    }
    public required Request Request { get; set; }
    public required bool Result { get; set; }
    
    /// <summary>
    /// This conversation will be null if the friend request is rejected.
    /// </summary>
    public required PrivateConversation? CreatedConversation { get; set; }
}