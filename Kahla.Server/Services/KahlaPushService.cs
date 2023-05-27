using Aiursoft.Observer.SDK.Services.ToObserverServer;
using Aiursoft.Scanner.Abstract;
using Aiursoft.XelNaga.Services;
using Kahla.SDK.Events;
using Kahla.SDK.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aiursoft.Directory.SDK.Services;

namespace Kahla.Server.Services
{
    public class KahlaPushService : IScopedDependency
    {
        private readonly AppsContainer _appsContainer;
        private readonly CannonService _cannonService;
        private readonly ObserverService _eventService;

        public KahlaPushService(
            AppsContainer appsContainer,
            CannonService cannonService,
            ObserverService eventService)
        {
            _appsContainer = appsContainer;
            _cannonService = cannonService;
            _eventService = eventService;
        }

        public void HandleError(Exception e)
        {
            _eventService.LogExceptionAsync(_appsContainer.GetAccessTokenAsync().Result, e, nameof(KahlaPushService)).Wait();
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
                _cannonService.FireAsync<StargatePushService>(s => s.PushMessageAsync(token, stargateChannel, newMessageEvent),  e => HandleError(e));
            }
            if (pushAlert)
            {
                _cannonService.FireAsync<ThirdPartyPushService>(s => s.PushAsync(devices, newMessageEvent, message.Sender.Email), (e) => HandleError(e));
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
                _cannonService.FireAsync<StargatePushService>(s => s.PushMessageAsync(token, target.CurrentChannel, newFriendRequestEvent),  (e) => HandleError(e));
            }
            _cannonService.FireAsync<ThirdPartyPushService>(s => s.PushAsync(target.HisDevices, newFriendRequestEvent),  (e) => HandleError(e));
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
                _cannonService.FireAsync<StargatePushService>(s => s.PushMessageAsync(token, target.CurrentChannel, friendAcceptedEvent), (e) => HandleError(e));
            }
            _cannonService.FireAsync<ThirdPartyPushService>(s => s.PushAsync(target.HisDevices, friendAcceptedEvent), (e) => HandleError(e));
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
                _cannonService.FireAsync<StargatePushService>(s => s.PushMessageAsync(token, stargateChannel, friendDeletedEvent), (e) => HandleError(e));
            }
            _cannonService.FireAsync<ThirdPartyPushService>(s => s.PushAsync(devices, friendDeletedEvent), (e) => HandleError(e));
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
                _cannonService.FireAsync<StargatePushService>(s => s.PushMessageAsync(token, channel, timerUpdatedEvent), (e) => HandleError(e));
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
                _cannonService.FireAsync<StargatePushService>(s => s.PushMessageAsync(token, channel, newMemberEvent), (e) => HandleError(e));
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
                _cannonService.FireAsync<StargatePushService>(s => s.PushMessageAsync(token, channel, someoneLeftEvent), (e) => HandleError(e));
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
                _cannonService.FireAsync<StargatePushService>(s => s.PushMessageAsync(token, channel, dissolvevent), (e) => HandleError(e));
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
                _cannonService.FireAsync<StargatePushService>(s => s.PushMessageAsync(token, channel, groupJoinedEvent), (e) => HandleError(e));
            }
        }
    }
}
