using Aiursoft.Pylon.Models;

namespace Kahla.Server.Events
{
    public enum EventType
    {
        NewMessage = 0,
        NewFriendRequestEvent = 1,
        WereDeletedEvent = 2,
        FriendAcceptedEvent = 3,
        TimerUpdatedEvent = 4,
    }
    public abstract class KahlaEvent
    {
        protected EventType Type { get; set; }
    }
    public class NewMessageEvent : KahlaEvent
    {
        public NewMessageEvent()
        {
            Type = EventType.NewMessage;
        }
        public int ConversationId { get; set; }
        public AiurUserBase Sender { get; set; }
        public string Content { get; set; }
        public string AESKey { get; set; }
        public bool Muted { get; set; }
    }
    public class NewFriendRequestEvent : KahlaEvent
    {
        public NewFriendRequestEvent()
        {
            Type = EventType.NewFriendRequestEvent;
        }
        public string RequesterId { get; set; }
    }
    public class WereDeletedEvent : KahlaEvent
    {
        public WereDeletedEvent()
        {
            Type = EventType.WereDeletedEvent;
        }
    }
    public class FriendAcceptedEvent : KahlaEvent
    {
        public FriendAcceptedEvent()
        {
            Type = EventType.FriendAcceptedEvent;
        }
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
}
