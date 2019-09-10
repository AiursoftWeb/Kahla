using Aiursoft.Pylon.Models.Stargate.ChannelViewModels;
using Aiursoft.Pylon.Services;
using Aiursoft.Pylon.Services.ToStargateServer;
using Kahla.Server.Events;
using Kahla.Server.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kahla.Server.Services
{
    public class KahlaPushService
    {
        private readonly PushMessageService _stargatePushService;
        private readonly AppsContainer _appsContainer;
        private readonly ChannelService _channelService;
        private readonly ThirdPartyPushService _thirdPartyPushService;

        public KahlaPushService(
            PushMessageService stargatePushService,
            AppsContainer appsContainer,
            ChannelService channelService,
            ThirdPartyPushService thirdPartyPushService)
        {
            _stargatePushService = stargatePushService;
            _appsContainer = appsContainer;
            _channelService = channelService;
            _thirdPartyPushService = thirdPartyPushService;
        }

        public async Task<CreateChannelViewModel> Init(string userId)
        {
            var token = await _appsContainer.AccessToken();
            var channel = await _channelService.CreateChannelAsync(token, $"Kahla User Channel for Id: {userId}");
            return channel;
        }

        public async Task NewMessageEvent(KahlaUser receiver, Conversation conversation, Message message, bool muted, bool mentioned)
        {
            var token = await _appsContainer.AccessToken();
            var channel = receiver.CurrentChannel;
            var newMessageEvent = new NewMessageEvent
            {
                AESKey = conversation.AESKey,
                Muted = muted,
                Mentioned = mentioned,
                Message = message
            };
            var pushTasks = new List<Task>();
            if (channel != -1)
            {
                pushTasks.Add(_stargatePushService.PushMessageAsync(token, channel, JsonConvert.SerializeObject(newMessageEvent), true));
            }
            if (!muted && receiver.Id != message.Sender.Id)
            {
                pushTasks.Add(_thirdPartyPushService.PushAsync(receiver.Id, message.Sender.Email, JsonConvert.SerializeObject(newMessageEvent)));
            }
            await Task.WhenAll(pushTasks);
        }

        public async Task NewFriendRequestEvent(KahlaUser receiver, KahlaUser requester)
        {
            var token = await _appsContainer.AccessToken();
            var channel = receiver.CurrentChannel;
            var newFriendRequestEvent = new NewFriendRequestEvent
            {
                RequesterId = requester.Id,
                Requester = requester
            };
            if (channel != -1)
            {
                await _stargatePushService.PushMessageAsync(token, channel, JsonConvert.SerializeObject(newFriendRequestEvent), true);
            }
            await _thirdPartyPushService.PushAsync(receiver.Id, requester.Email, JsonConvert.SerializeObject(newFriendRequestEvent));
        }

        public async Task WereDeletedEvent(KahlaUser receiver, KahlaUser trigger)
        {
            var token = await _appsContainer.AccessToken();
            var channel = receiver.CurrentChannel;
            var wereDeletedEvent = new WereDeletedEvent
            {
                Trigger = trigger
            };
            if (channel != -1)
            {
                await _stargatePushService.PushMessageAsync(token, channel, JsonConvert.SerializeObject(wereDeletedEvent), true);
            }
            await _thirdPartyPushService.PushAsync(receiver.Id, "postermaster@aiursoft.com", JsonConvert.SerializeObject(wereDeletedEvent));
        }

        public async Task FriendAcceptedEvent(KahlaUser receiver, KahlaUser accepter)
        {
            var token = await _appsContainer.AccessToken();
            var channel = receiver.CurrentChannel;
            var friendAcceptedEvent = new FriendAcceptedEvent
            {
                Target = accepter
            };
            if (channel != -1)
            {
                await _stargatePushService.PushMessageAsync(token, channel, JsonConvert.SerializeObject(friendAcceptedEvent), true);
            }
            await _thirdPartyPushService.PushAsync(receiver.Id, "postermaster@aiursoft.com", JsonConvert.SerializeObject(friendAcceptedEvent));
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
            if (channel != -1)
            {
                await _stargatePushService.PushMessageAsync(token, channel, JsonConvert.SerializeObject(timerUpdatedEvent), true);
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
            if (channel != -1)
            {
                await _stargatePushService.PushMessageAsync(token, channel, JsonConvert.SerializeObject(newMemberEvent), true);
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
            if (channel != -1)
            {
                await _stargatePushService.PushMessageAsync(token, channel, JsonConvert.SerializeObject(someoneLeftEvent), true);
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

            if (channel != -1)
            {
                await _stargatePushService.PushMessageAsync(token, channel, JsonConvert.SerializeObject(dissolvevent), true);
            }
        }
    }
}
