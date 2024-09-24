using Aiursoft.Kahla.SDK.Models;

namespace Aiursoft.Kahla.SDK.Events
{
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
    public class KahlaEvent
    {
        public required EventType Type { get; set; }
        public string TypeDescription => Type.ToString();
    }

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

    public class NewFriendRequestEvent : KahlaEvent
    {
        public NewFriendRequestEvent()
        {
            Type = EventType.NewFriendRequestEvent;
        }

        public required Request Request { get; set; }
    }

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

    public class FriendDeletedEvent : KahlaEvent
    {
        public FriendDeletedEvent()
        {
            Type = EventType.FriendDeletedEvent;
        }

        public required  KahlaUser Trigger { get; set; }
        public required int ConversationId { get; init; }
    }

    public class TimerUpdatedEvent : KahlaEvent
    {
        public TimerUpdatedEvent()
        {
            Type = EventType.TimerUpdatedEvent;
        }
        public int ConversationId { get; set; }
        public int NewTimer { get; set; }
    }

    public class NewMemberEvent : KahlaEvent
    {
        public NewMemberEvent()
        {
            Type = EventType.NewMemberEvent;
        }
        public required int ConversationId { get; set; }
        public required KahlaUser NewMember { get; set; }
    }

    public class SomeoneLeftEvent : KahlaEvent
    {
        public SomeoneLeftEvent()
        {
            Type = EventType.SomeoneLeftEvent;
        }
        public required int ConversationId { get; set; }
        public required KahlaUser LeftUser { get; set; }
    }

    public class DissolveEvent : KahlaEvent
    {
        public DissolveEvent()
        {
            Type = EventType.DissolveEvent;
        }

        public int ConversationId { get; set; }
    }

    public class GroupJoinedEvent : KahlaEvent
    {
        public GroupJoinedEvent()
        {
            Type = EventType.GroupJoinedEvent;
        }

        public required GroupConversation CreatedConversation { get; set; }
        public required Message LatestMessage { get; set; }
        public int MessageCount { get; set; }
    }
}
