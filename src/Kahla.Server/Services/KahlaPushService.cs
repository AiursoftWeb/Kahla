using Kahla.SDK.Events;
using Kahla.SDK.Models;
using Aiursoft.Canon;
using Aiursoft.Directory.SDK.Services;
using Aiursoft.Scanner.Abstractions;

namespace Kahla.Server.Services
{
    public class KahlaPushService : IScopedDependency
    {
        private readonly DirectoryAppTokenService _appsContainer;
        private readonly CanonService _cannonService;

        public KahlaPushService(
            DirectoryAppTokenService appsContainer,
            CanonService cannonService)
        {
            _appsContainer = appsContainer;
            _cannonService = cannonService;
        }

        public async Task NewMessageEvent(int stargateChannel, IEnumerable<Device> devices, Conversation conversation, Message message, string lastMessageId, bool pushAlert, bool mentioned)
        {
            var token = await _appsContainer.GetAccessTokenAsync();
            var newMessageEvent = new NewMessageEvent
            {
                AESKey = conversation.AESKey,
                Muted = !pushAlert,
                Mentioned = mentioned,
                Message = message,
                PreviousMessageId = lastMessageId
            };
            if (stargateChannel > 0)
            {
                _cannonService.FireAsync<StargatePushService>(s => s.PushMessageAsync(token, stargateChannel, newMessageEvent));
            }
            if (pushAlert)
            {
                _cannonService.FireAsync<ThirdPartyPushService>(s => s.PushAsync(devices, newMessageEvent, message.Sender.Email));
            }
        }

        public async Task NewFriendRequestEvent(KahlaUser target, Request request)
        {
            var token = await _appsContainer.GetAccessTokenAsync();
            var newFriendRequestEvent = new NewFriendRequestEvent
            {
                Request = request
            };
            if (target.CurrentChannel > 0)
            {
                _cannonService.FireAsync<StargatePushService>(s => s.PushMessageAsync(token, target.CurrentChannel, newFriendRequestEvent));
            }
            _cannonService.FireAsync<ThirdPartyPushService>(s => s.PushAsync(target.HisDevices, newFriendRequestEvent));
        }

        public async Task FriendsChangedEvent(KahlaUser target, Request request, bool result, PrivateConversation conversation)
        {
            var token = await _appsContainer.GetAccessTokenAsync();
            var friendAcceptedEvent = new FriendsChangedEvent
            {
                Request = request,
                Result = result,
                CreatedConversation = conversation
            };
            if (target.CurrentChannel > 0)
            {
                _cannonService.FireAsync<StargatePushService>(s => s.PushMessageAsync(token, target.CurrentChannel, friendAcceptedEvent));
            }
            _cannonService.FireAsync<ThirdPartyPushService>(s => s.PushAsync(target.HisDevices, friendAcceptedEvent));
        }

        public async Task FriendDeletedEvent(int stargateChannel, IEnumerable<Device> devices, KahlaUser trigger, int deletedConversationId)
        {
            var token = await _appsContainer.GetAccessTokenAsync();
            var friendDeletedEvent = new FriendDeletedEvent
            {
                Trigger = trigger,
                ConversationId = deletedConversationId
            };
            if (stargateChannel > 0)
            {
                _cannonService.FireAsync<StargatePushService>(s => s.PushMessageAsync(token, stargateChannel, friendDeletedEvent));
            }
            _cannonService.FireAsync<ThirdPartyPushService>(s => s.PushAsync(devices, friendDeletedEvent));
        }

        public async Task TimerUpdatedEvent(KahlaUser receiver, int newTimer, int conversationId)
        {
            var token = await _appsContainer.GetAccessTokenAsync();
            var channel = receiver.CurrentChannel;
            var timerUpdatedEvent = new TimerUpdatedEvent
            {
                NewTimer = newTimer,
                ConversationId = conversationId
            };
            if (channel > 0)
            {
                _cannonService.FireAsync<StargatePushService>(s => s.PushMessageAsync(token, channel, timerUpdatedEvent));
            }
        }

        public async Task NewMemberEvent(KahlaUser receiver, KahlaUser newMember, int conversationId)
        {
            var token = await _appsContainer.GetAccessTokenAsync();
            var channel = receiver.CurrentChannel;
            var newMemberEvent = new NewMemberEvent
            {
                NewMember = newMember,
                ConversationId = conversationId
            };
            if (channel > 0)
            {
                _cannonService.FireAsync<StargatePushService>(s => s.PushMessageAsync(token, channel, newMemberEvent));
            }
        }

        public async Task SomeoneLeftEvent(KahlaUser receiver, KahlaUser leftMember, int conversationId)
        {
            var token = await _appsContainer.GetAccessTokenAsync();
            var channel = receiver.CurrentChannel;
            var someoneLeftEvent = new SomeoneLeftEvent
            {
                LeftUser = leftMember,
                ConversationId = conversationId
            };
            if (channel > 0)
            {
                _cannonService.FireAsync<StargatePushService>(s => s.PushMessageAsync(token, channel, someoneLeftEvent));
            }
        }

        public async Task DissolveEvent(KahlaUser receiver, int conversationId)
        {
            var token = await _appsContainer.GetAccessTokenAsync();
            var channel = receiver.CurrentChannel;
            var dissolvevent = new DissolveEvent
            {
                ConversationId = conversationId
            };

            if (channel > 0)
            {
                _cannonService.FireAsync<StargatePushService>(s => s.PushMessageAsync(token, channel, dissolvevent));
            }
        }

        public async Task GroupJoinedEvent(KahlaUser receiver, GroupConversation createdConversation, Message latestMessage, int messageCount)
        {
            var token = await _appsContainer.GetAccessTokenAsync();
            var channel = receiver.CurrentChannel;
            var groupJoinedEvent = new GroupJoinedEvent
            {
                CreatedConversation = createdConversation,
                LatestMessage = latestMessage,
                MessageCount = messageCount
            };

            if (channel > 0)
            {
                _cannonService.FireAsync<StargatePushService>(s => s.PushMessageAsync(token, channel, groupJoinedEvent));
            }
        }
    }
}
