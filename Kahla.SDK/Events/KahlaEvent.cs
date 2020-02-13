using Kahla.SDK.Models;

namespace Kahla.SDK.Events
{
    public enum EventType
    {
        NewMessage = 0,
        NewFriendRequestEvent = 1,
        WereDeletedEvent = 2,
        FriendAcceptedEvent = 3,
        TimerUpdatedEvent = 4,
        NewMemberEvent = 5,
        SomeoneLeftEvent = 6,
        DissolveEvent = 7,
    }
    public class KahlaEvent
    {
        public EventType Type { get; set; }
        public string TypeDescription => Type.ToString();
    }
    public class NewMessageEvent : KahlaEvent
    {
        public NewMessageEvent()
        {
            Type = EventType.NewMessage;
        }
        public string AESKey { get; set; }
        public bool Muted { get; set; }
        public bool Mentioned { get; set; }
        public int ConversationId => Message.ConversationId;
        public Message Message { get; set; }
    }
    public class NewFriendRequestEvent : KahlaEvent
    {
        public NewFriendRequestEvent()
        {
            Type = EventType.NewFriendRequestEvent;
        }
        public string RequesterId { get; set; }
        public KahlaUser Requester { get; set; }
        public int RequestId { get; set; }
    }
    public class WereDeletedEvent : KahlaEvent
    {
        public WereDeletedEvent()
        {
            Type = EventType.WereDeletedEvent;
        }
        public KahlaUser Trigger { get; set; }
    }
    public class FriendAcceptedEvent : KahlaEvent
    {
        public FriendAcceptedEvent()
        {
            Type = EventType.FriendAcceptedEvent;
        }
        public KahlaUser Target { get; set; }
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
        public int ConversationId { get; set; }
        public KahlaUser NewMember { get; set; }
    }

    public class SomeoneLeftEvent : KahlaEvent
    {
        public SomeoneLeftEvent()
        {
            Type = EventType.SomeoneLeftEvent;
        }
        public int ConversationId { get; set; }
        public KahlaUser LeftUser { get; set; }
    }

    public class DissolveEvent : KahlaEvent
    {
        public DissolveEvent()
        {
            Type = EventType.DissolveEvent;
        }

        public int ConversationId { get; set; }
    }
}
