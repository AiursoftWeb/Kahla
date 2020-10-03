using Aiursoft.Archon.SDK.Services;
using Aiursoft.Scanner.Interfaces;
using Aiursoft.Stargate.SDK.Models.ChannelViewModels;
using Aiursoft.Stargate.SDK.Services.ToStargateServer;
using Aiursoft.XelNaga.Services;
using Kahla.SDK.Events;
using Kahla.SDK.Models;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kahla.Server.Services
{
    public class KahlaPushService : IScopedDependency
    {
        private readonly AppsContainer _appsContainer;
        private readonly ChannelService _channelService;
        private readonly CannonService _cannonService;

        public KahlaPushService(
            AppsContainer appsContainer,
            ChannelService channelService,
            CannonService cannonService)
        {
            _appsContainer = appsContainer;
            _channelService = channelService;
            _cannonService = cannonService;
        }

        public async Task<CreateChannelViewModel> ReCreateStargateChannel(string userId)
        {
            var token = await _appsContainer.AccessToken();
            var channel = await _channelService.CreateChannelAsync(token, $"Kahla User Channel for Id: {userId}");
            return channel;
        }

        public async Task NewMessageEvent(int stargateChannel, IEnumerable<Device> devices, Conversation conversation, Message message, string lastMessageId, bool pushAlert, bool mentioned)
        {
            var token = await _appsContainer.AccessToken();
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
                _cannonService.FireAsync<PushMessageService>(s => s.PushMessageAsync(token, stargateChannel, JsonConvert.SerializeObject(newMessageEvent), true));
            }
            if (pushAlert)
            {
                _cannonService.FireAsync<ThirdPartyPushService>(s => s.PushAsync(devices, newMessageEvent, message.Sender.Email));
            }
        }

        public async Task NewFriendRequestEvent(KahlaUser target, Request request)
        {
            var token = await _appsContainer.AccessToken();
            var newFriendRequestEvent = new NewFriendRequestEvent
            {
                Request = request
            };
            if (target.CurrentChannel > 0)
            {
                _cannonService.FireAsync<PushMessageService>(s => s.PushMessageAsync(token, target.CurrentChannel, JsonConvert.SerializeObject(newFriendRequestEvent), true));
            }
            _cannonService.FireAsync<ThirdPartyPushService>(s => s.PushAsync(target.HisDevices, newFriendRequestEvent));
        }

        public async Task FriendsChangedEvent(KahlaUser target, Request request, bool result, PrivateConversation conversation)
        {
            var token = await _appsContainer.AccessToken();
            var friendAcceptedEvent = new FriendsChangedEvent
            {
                Request = request,
                Result = result,
                CreatedConversation = conversation
            };
            if (target.CurrentChannel > 0)
            {
                _cannonService.FireAsync<PushMessageService>(s => s.PushMessageAsync(token, target.CurrentChannel, JsonConvert.SerializeObject(friendAcceptedEvent), true));
            }
            _cannonService.FireAsync<ThirdPartyPushService>(s => s.PushAsync(target.HisDevices, friendAcceptedEvent));
        }

        public async Task FriendDeletedEvent(int stargateChannel, IEnumerable<Device> devices, KahlaUser trigger, int deletedConversationId)
        {
            var token = await _appsContainer.AccessToken();
            var friendDeletedEvent = new FriendDeletedEvent
            {
                Trigger = trigger,
                ConversationId = deletedConversationId
            };
            if (stargateChannel > 0)
            {
                _cannonService.FireAsync<PushMessageService>(s => s.PushMessageAsync(token, stargateChannel, JsonConvert.SerializeObject(friendDeletedEvent), true));
            }
            _cannonService.FireAsync<ThirdPartyPushService>(s => s.PushAsync(devices, friendDeletedEvent));
        }

        public async Task TimerUpdatedEvent(KahlaUser receiver, int newTimer, int conversationId)
        {
            var token = await _appsContainer.AccessToken();
            var channel = receiver.CurrentChannel;
            var timerUpdatedEvent = new TimerUpdatedEvent
            {
                NewTimer = newTimer,
                ConversationId = conversationId
            };
            if (channel > 0)
            {
                _cannonService.FireAsync<PushMessageService>(s => s.PushMessageAsync(token, channel, JsonConvert.SerializeObject(timerUpdatedEvent), true));
            }
        }

        public async Task NewMemberEvent(KahlaUser receiver, KahlaUser newMember, int conversationId)
        {
            var token = await _appsContainer.AccessToken();
            var channel = receiver.CurrentChannel;
            var newMemberEvent = new NewMemberEvent
            {
                NewMember = newMember,
                ConversationId = conversationId
            };
            if (channel > 0)
            {
                _cannonService.FireAsync<PushMessageService>(s => s.PushMessageAsync(token, channel, JsonConvert.SerializeObject(newMemberEvent), true));
            }
        }

        public async Task SomeoneLeftEvent(KahlaUser receiver, KahlaUser leftMember, int conversationId)
        {
            var token = await _appsContainer.AccessToken();
            var channel = receiver.CurrentChannel;
            var someoneLeftEvent = new SomeoneLeftEvent
            {
                LeftUser = leftMember,
                ConversationId = conversationId
            };
            if (channel > 0)
            {
                _cannonService.FireAsync<PushMessageService>(s => s.PushMessageAsync(token, channel, JsonConvert.SerializeObject(someoneLeftEvent), true));
            }
        }

        public async Task DissolveEvent(KahlaUser receiver, int conversationId)
        {
            var token = await _appsContainer.AccessToken();
            var channel = receiver.CurrentChannel;
            var dissolvevent = new DissolveEvent
            {
                ConversationId = conversationId
            };

            if (channel > 0)
            {
                _cannonService.FireAsync<PushMessageService>(s => s.PushMessageAsync(token, channel, JsonConvert.SerializeObject(dissolvevent), true));
            }
        }

        public async Task GroupJoinedEvent(KahlaUser receiver, GroupConversation createdConversation, Message latestMessage, int messageCount)
        {
            var token = await _appsContainer.AccessToken();
            var channel = receiver.CurrentChannel;
            var groupJoinedEvent = new GroupJoinedEvent
            {
                CreatedConversation = createdConversation,
                LatestMessage = latestMessage,
                MessageCount = messageCount
            };

            if (channel > 0)
            {
                _cannonService.FireAsync<PushMessageService>(s => s.PushMessageAsync(token, channel, JsonConvert.SerializeObject(groupJoinedEvent), true));
            }
        }
    }
}
