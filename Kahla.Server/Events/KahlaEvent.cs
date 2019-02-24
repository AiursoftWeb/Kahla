using Aiursoft.Pylon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.Server.Events
{
    public enum EventType : int
    {
        NewMessage = 0,
        NewFriendRequest = 1,
        WereDeletedEvent = 2,
        FriendAcceptedEvent = 3
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
        public bool SentByMe { get; set; }
    }
    public class NewFriendRequest : KahlaEvent
    {
        public string RequesterId { get; set; }
    }
    public class WereDeletedEvent : KahlaEvent
    {

    }
    public class FriendAcceptedEvent : KahlaEvent
    {

    }
}
