using Aiursoft.Pylon.Models;

namespace Kahla.Server.Events
{
    public enum EventType
    {
        NewMessage = 0,
        NewFriendRequestEvent = 1,
        WereDeletedEvent = 2,
        FriendAcceptedEvent = 3,
        TimerUpdatedEvent = 3,
    }
    public abstract class KahlaEvent
    {
        public EventType Type { get; set; }
    }
    public class NewMessageEvent : KahlaEvent
    {
        public int ConversationId { get; set; }
        public AiurUserBase Sender { get; set; }
        public string Content { get; set; }
        public string AESKey { get; set; }
        public bool Muted { get; set; }
    }
    public class NewFriendRequestEvent : KahlaEvent
    {
        public string RequesterId { get; set; }
    }
    public class WereDeletedEvent : KahlaEvent
    {

    }
    public class FriendAcceptedEvent : KahlaEvent
    {

    }

    public class TimerUpdatedEvent : KahlaEvent
    {
        public int NewTimer { get; set; }
    }
}
