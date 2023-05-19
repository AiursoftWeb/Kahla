using Aiursoft.Observer.SDK.Services.ToObserverServer;
using Aiursoft.Scanner.Abstract;
using Aiursoft.XelNaga.Services;
using Kahla.SDK.Events;
using Kahla.SDK.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aiursoft.Gateway.SDK.Services;

namespace Kahla.Server.Services
{
    public class KahlaPushService : IScopedDependency
    {
        private readonly AppsContainer _appsContainer;
        private readonly CannonService _cannonService;
        private readonly EventService _eventService;

        public KahlaPushService(
            AppsContainer appsContainer,
            CannonService cannonService,
            EventService eventService)
        {
            _appsContainer = appsContainer;
            _cannonService = cannonService;
            _eventService = eventService;
        }

        public async Task HandleError(Exception e)
        {
            await _eventService.LogExceptionAsync(await _appsContainer.AccessTokenAsync(), e, nameof(KahlaPushService));
        }

        public async Task NewMessageEvent(int stargateChannel, IEnumerable<Device> devices, Conversation conversation, Message message, string lastMessageId, bool pushAlert, bool mentioned)
        {
            var token = await _appsContainer.AccessTokenAsync();
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
                _cannonService.FireAsync<StargatePushService>(s => s.PushMessageAsync(token, stargateChannel, newMessageEvent), async (e) => await HandleError(e));
            }
            if (pushAlert)
            {
                _cannonService.FireAsync<ThirdPartyPushService>(s => s.PushAsync(devices, newMessageEvent, message.Sender.Email), async (e) => await HandleError(e));
            }
        }

        public async Task NewFriendRequestEvent(KahlaUser target, Request request)
        {
            var token = await _appsContainer.AccessTokenAsync();
            var newFriendRequestEvent = new NewFriendRequestEvent
            {
                Request = request
            };
            if (target.CurrentChannel > 0)
            {
                _cannonService.FireAsync<StargatePushService>(s => s.PushMessageAsync(token, target.CurrentChannel, newFriendRequestEvent), async (e) => await HandleError(e));
            }
            _cannonService.FireAsync<ThirdPartyPushService>(s => s.PushAsync(target.HisDevices, newFriendRequestEvent), async (e) => await HandleError(e));
        }

        public async Task FriendsChangedEvent(KahlaUser target, Request request, bool result, PrivateConversation conversation)
        {
            var token = await _appsContainer.AccessTokenAsync();
            var friendAcceptedEvent = new FriendsChangedEvent
            {
                Request = request,
                Result = result,
                CreatedConversation = conversation
            };
            if (target.CurrentChannel > 0)
            {
                _cannonService.FireAsync<StargatePushService>(s => s.PushMessageAsync(token, target.CurrentChannel, friendAcceptedEvent), async (e) => await HandleError(e));
            }
            _cannonService.FireAsync<ThirdPartyPushService>(s => s.PushAsync(target.HisDevices, friendAcceptedEvent), async (e) => await HandleError(e));
        }

        public async Task FriendDeletedEvent(int stargateChannel, IEnumerable<Device> devices, KahlaUser trigger, int deletedConversationId)
        {
            var token = await _appsContainer.AccessTokenAsync();
            var friendDeletedEvent = new FriendDeletedEvent
            {
                Trigger = trigger,
                ConversationId = deletedConversationId
            };
            if (stargateChannel > 0)
            {
                _cannonService.FireAsync<StargatePushService>(s => s.PushMessageAsync(token, stargateChannel, friendDeletedEvent), async (e) => await HandleError(e));
            }
            _cannonService.FireAsync<ThirdPartyPushService>(s => s.PushAsync(devices, friendDeletedEvent), async (e) => await HandleError(e));
        }

        public async Task TimerUpdatedEvent(KahlaUser receiver, int newTimer, int conversationId)
        {
            var token = await _appsContainer.AccessTokenAsync();
            var channel = receiver.CurrentChannel;
            var timerUpdatedEvent = new TimerUpdatedEvent
            {
                NewTimer = newTimer,
                ConversationId = conversationId
            };
            if (channel > 0)
            {
                _cannonService.FireAsync<StargatePushService>(s => s.PushMessageAsync(token, channel, timerUpdatedEvent), async (e) => await HandleError(e));
            }
        }

        public async Task NewMemberEvent(KahlaUser receiver, KahlaUser newMember, int conversationId)
        {
            var token = await _appsContainer.AccessTokenAsync();
            var channel = receiver.CurrentChannel;
            var newMemberEvent = new NewMemberEvent
            {
                NewMember = newMember,
                ConversationId = conversationId
            };
            if (channel > 0)
            {
                _cannonService.FireAsync<StargatePushService>(s => s.PushMessageAsync(token, channel, newMemberEvent), async (e) => await HandleError(e));
            }
        }

        public async Task SomeoneLeftEvent(KahlaUser receiver, KahlaUser leftMember, int conversationId)
        {
            var token = await _appsContainer.AccessTokenAsync();
            var channel = receiver.CurrentChannel;
            var someoneLeftEvent = new SomeoneLeftEvent
            {
                LeftUser = leftMember,
                ConversationId = conversationId
            };
            if (channel > 0)
            {
                _cannonService.FireAsync<StargatePushService>(s => s.PushMessageAsync(token, channel, someoneLeftEvent), async (e) => await HandleError(e));
            }
        }

        public async Task DissolveEvent(KahlaUser receiver, int conversationId)
        {
            var token = await _appsContainer.AccessTokenAsync();
            var channel = receiver.CurrentChannel;
            var dissolvevent = new DissolveEvent
            {
                ConversationId = conversationId
            };

            if (channel > 0)
            {
                _cannonService.FireAsync<StargatePushService>(s => s.PushMessageAsync(token, channel, dissolvevent), async (e) => await HandleError(e));
            }
        }

        public async Task GroupJoinedEvent(KahlaUser receiver, GroupConversation createdConversation, Message latestMessage, int messageCount)
        {
            var token = await _appsContainer.AccessTokenAsync();
            var channel = receiver.CurrentChannel;
            var groupJoinedEvent = new GroupJoinedEvent
            {
                CreatedConversation = createdConversation,
                LatestMessage = latestMessage,
                MessageCount = messageCount
            };

            if (channel > 0)
            {
                _cannonService.FireAsync<StargatePushService>(s => s.PushMessageAsync(token, channel, groupJoinedEvent), async (e) => await HandleError(e));
            }
        }
    }
}
