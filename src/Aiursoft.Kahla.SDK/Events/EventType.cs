namespace Aiursoft.Kahla.SDK.Events;

public enum EventType
{
    /// <summary>
    /// When someone sent you a new message.
    /// </summary>
    NewMessage = 0,
    /// <summary>
    /// When a friend request related to you was created.
    /// </summary>
    NewFriendRequestEvent = 1,
    /// <summary>
    /// When you were deleted by a friend or you deleted a friend.
    /// </summary>
    FriendDeletedEvent = 2,
    /// <summary>
    /// When one of friend request related to you was completed.
    /// </summary>
    FriendsChangedEvent = 3,
    /// <summary>
    /// When the timer of one of the conversations you joined was changed.
    /// </summary>
    TimerUpdatedEvent = 4,
    /// <summary>
    /// When someone joined a group you joined.
    /// </summary>
    NewMemberEvent = 5,
    /// <summary>
    /// When someone left a group you joined or kicked out of a group.
    /// </summary>
    SomeoneLeftEvent = 6,
    /// <summary>
    /// When the group owner dissolved the group.
    /// </summary>
    DissolveEvent = 7,
    /// <summary>
    /// When you successfully joined a group.
    /// </summary>
    GroupJoinedEvent = 8
}