using System.Collections.Generic;
using System.Threading.Tasks;
using Aiursoft.Pylon.Services;
using Aiursoft.Pylon.Services.ToStargateServer;
using Aiursoft.Pylon.Models.Stargate.ChannelViewModels;
using Kahla.Server.Data;
using Kahla.Server.Events;
using Newtonsoft.Json;
using Kahla.Server.Models;
using Newtonsoft.Json.Serialization;

namespace Kahla.Server.Services
{
    public class KahlaPushService
    {
        private readonly KahlaDbContext _dbContext;
        private readonly PushMessageService _stargatePushService;
        private readonly AppsContainer _appsContainer;
        private readonly ChannelService _channelService;
        private readonly ThirdPartyPushService _thirdPartyPushService;

        public KahlaPushService(
            KahlaDbContext dbContext,
            PushMessageService stargatePushService,
            AppsContainer appsContainer,
            ChannelService channelService,
            ThirdPartyPushService thirdPartyPushService)
        {
            _dbContext = dbContext;
            _stargatePushService = stargatePushService;
            _appsContainer = appsContainer;
            _channelService = channelService;
            _thirdPartyPushService = thirdPartyPushService;
        }

        private static string _Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }

        public async Task<CreateChannelViewModel> Init(string userId)
        {
            var token = await _appsContainer.AccessToken();
            var channel = await _channelService.CreateChannelAsync(token, $"Kahla User Channel for Id: {userId}");
            return channel;
        }

        public async Task NewMessageEvent(KahlaUser receiver, Conversation conversation, string content, KahlaUser sender, bool alert)
        {
            var token = await _appsContainer.AccessToken();
            var channel = receiver.CurrentChannel;
            var newEvent = new NewMessageEvent
            {
                Type = EventType.NewMessage,
                ConversationId = conversation.Id,
                Sender = sender,
                Content = content,
                AESKey = conversation.AESKey,
                Muted = !alert,
                SentByMe = receiver.Id == sender.Id
            };
            var pushTasks = new List<Task>();
            if (channel != -1)
            {
                pushTasks.Add(_stargatePushService.PushMessageAsync(token, channel, _Serialize(newEvent), true));
            }
            if (alert)
            {
                pushTasks.Add(_thirdPartyPushService.PushAsync(receiver.Id, sender.Email, _Serialize(newEvent)));
            }
            await Task.WhenAll(pushTasks);
        }

        public async Task NewFriendRequestEvent(string receiverId, string requesterId)
        {
            var token = await _appsContainer.AccessToken();
            var receiver = await _dbContext.Users.FindAsync(receiverId);
            var requester = await _dbContext.Users.FindAsync(requesterId);
            var channel = receiver.CurrentChannel;
            var newEvent = new NewFriendRequestEvent
            {
                Type = EventType.NewFriendRequestEvent,
                RequesterId = requesterId
            };
            if (channel != -1)
                await _stargatePushService.PushMessageAsync(token, channel, _Serialize(newEvent), true);
            await _thirdPartyPushService.PushAsync(receiver.Id, requester.Email, _Serialize(newEvent));
        }

        public async Task WereDeletedEvent(string recieverId)
        {
            var token = await _appsContainer.AccessToken();
            var user = await _dbContext.Users.FindAsync(recieverId);
            var channel = user.CurrentChannel;
            var nevent = new WereDeletedEvent
            {
                Type = EventType.WereDeletedEvent
            };
            if (channel != -1)
                await _stargatePushService.PushMessageAsync(token, channel, _Serialize(nevent), true);
            await _thirdPartyPushService.PushAsync(user.Id, "postermaster@aiursoft.com", _Serialize(nevent));
        }

        public async Task FriendAcceptedEvent(string recieverId)
        {
            var token = await _appsContainer.AccessToken();
            var user = await _dbContext.Users.FindAsync(recieverId);
            var channel = user.CurrentChannel;
            var nevent = new FriendAcceptedEvent
            {
                Type = EventType.FriendAcceptedEvent
            };
            if (channel != -1)
                await _stargatePushService.PushMessageAsync(token, channel, _Serialize(nevent), true);
            await _thirdPartyPushService.PushAsync(user.Id, "postermaster@aiursoft.com", _Serialize(nevent));
        }
    }
}
