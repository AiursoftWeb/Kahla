using Aiursoft.Canon;
using Aiursoft.Kahla.SDK.Events;
using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Models.Conversations;
using Aiursoft.Kahla.SDK.ModelsOBS;

namespace Aiursoft.Kahla.Server.Services
{
    public class KahlaPushService
    {
        private readonly CanonService _canon;

        public KahlaPushService(
            CanonService canon)
        {
            _canon = canon;
        }

        public void NewMessageEvent(KahlaUser user, Message message, bool muted,
            bool mentioned)
        {
            var newMessageEvent = new NewMessageEvent()
            {
                Muted = muted,
                Mentioned = mentioned,
                Message = message,
            };
            _canon.FireAsync<WebSocketPushService>(s => s.PushAsync(user, newMessageEvent));
            if (!muted)
            {
                _canon.FireAsync<WebPushService>(s => s.PushAsync(
                    devices: user.HisDevices,
                    payload: newMessageEvent,
                    triggerEmail: message.Sender?.Email ?? "unknown@domain.com"));
            }
        }

        public void NewFriendRequestEvent(KahlaUser target, Request request)
        {
            var newFriendRequestEvent = new NewFriendRequestEvent
            {
                Request = request
            };
            _canon.FireAsync<WebSocketPushService>(s => s.PushAsync(target, newFriendRequestEvent));
            _canon.FireAsync<WebPushService>(s => s.PushAsync(
                devices: target.HisDevices,
                payload: newFriendRequestEvent,
                triggerEmail: request.Creator?.Email ?? "unknown@domain.com"));
        }

        public void FriendRequestCompletedEvent(
            KahlaUser target, 
            Request request, 
            bool result,
            PrivateConversation? conversation)
        {
            var friendAcceptedEvent = new FriendRequestCompletedEvent
            {
                Request = request,
                Result = result,
                CreatedConversation = conversation
            };
            _canon.FireAsync<WebSocketPushService>(s => s.PushAsync(target, friendAcceptedEvent));
            _canon.FireAsync<WebPushService>(s => s.PushAsync(
                devices: target.HisDevices,
                payload: friendAcceptedEvent,
                triggerEmail: request.Creator?.Email ?? "unknown@domain.com"));
        }

        public void FriendDeletedEvent(KahlaUser target, KahlaUser trigger, int deletedConversationId)
        {
            var friendDeletedEvent = new FriendDeletedEvent
            {
                Trigger = trigger,
                ConversationId = deletedConversationId
            };
            _canon.FireAsync<WebSocketPushService>(s => s.PushAsync(trigger, friendDeletedEvent));
            _canon.FireAsync<WebPushService>(s => s.PushAsync(
                devices: target.HisDevices,
                payload: friendDeletedEvent,
                triggerEmail: trigger.Email ?? "unknown@domain.com"));
        }

        public void NewMemberEvent(KahlaUser receiver, KahlaUser newMember, int conversationId)
        {
            var newMemberEvent = new NewMemberEvent
            {
                NewMember = newMember,
                ConversationId = conversationId
            };
            _canon.FireAsync<WebSocketPushService>(s => s.PushAsync(receiver, newMemberEvent));
        }

        public void SomeoneLeftEvent(KahlaUser receiver, KahlaUser leftMember, int conversationId)
        {
            var someoneLeftEvent = new SomeoneLeftEvent
            {
                LeftUser = leftMember,
                ConversationId = conversationId
            };
            _canon.FireAsync<WebSocketPushService>(s => s.PushAsync(receiver, someoneLeftEvent));
        }

        public void DissolveEvent(KahlaUser receiver, int conversationId)
        {
            var dissolveEvent = new DissolveEvent
            {
                ConversationId = conversationId
            };
            _canon.FireAsync<WebSocketPushService>(s => s.PushAsync(receiver, dissolveEvent));
        }

        public void GroupJoinedEvent(KahlaUser receiver, GroupConversation createdConversation,
            Message latestMessage, int messageCount)
        {
            var groupJoinedEvent = new GroupJoinedEvent
            {
                CreatedConversation = createdConversation,
                LatestMessage = latestMessage,
                MessageCount = messageCount
            };

            _canon.FireAsync<WebSocketPushService>(s =>
                s.PushAsync(receiver, groupJoinedEvent));
        }
    }
}