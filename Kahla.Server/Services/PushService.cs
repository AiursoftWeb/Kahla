using System;
using System.Collections.Generic;
using System.Linq;
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
    public class PushKahlaMessageService
    {
        private readonly KahlaDbContext _dbContext;
        private readonly PushMessageService _pushMessageService;
        private readonly Aiursoft.Pylon.Models.AppsContainer _appsContainer;
        private readonly ChannelService _channelService;

        public PushKahlaMessageService(
            KahlaDbContext dbContext,
            PushMessageService pushMessageService,
            Aiursoft.Pylon.Models.AppsContainer appsContainer,
            ChannelService channelService)
        {
            _dbContext = dbContext;
            _pushMessageService = pushMessageService;
            _appsContainer = appsContainer;
            _channelService = channelService;
        }

        private string _CammalSer(object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }

        public async Task<CreateChannelViewModel> Init(string userId)
        {
            var token = await _appsContainer.AccessToken();
            var channel = await _channelService.CreateChannelAsync(token, $"Kahla User Channel for Id:{userId}");
            return channel;
        }

        public async Task NewMessageEvent(string recieverId, int conversationId, string content, KahlaUser sender, string aesKey, bool muted = false)
        {
            var token = await _appsContainer.AccessToken();
            var user = await _dbContext.Users.FindAsync(recieverId);
            var channel = user.CurrentChannel;
            var nevent = new NewMessageEvent
            {
                Type = EventType.NewMessage,
                ConversationId = conversationId,
                Sender = sender,
                Content = content,
                AESKey = aesKey,
                Muted = muted
            };
            if (channel != -1)
                await _pushMessageService.PushMessageAsync(token, channel, _CammalSer(nevent), true);
        }

        public async Task NewFriendRequestEvent(string recieverId, string requesterId)
        {
            var token = await _appsContainer.AccessToken();
            var user = await _dbContext.Users.FindAsync(recieverId);
            var channel = user.CurrentChannel;
            var nevent = new NewFriendRequest
            {
                Type = EventType.NewFriendRequest,
                RequesterId = requesterId
            };
            if (channel != -1)
                await _pushMessageService.PushMessageAsync(token, channel, _CammalSer(nevent), true);
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
                await _pushMessageService.PushMessageAsync(token, channel, _CammalSer(nevent), true);
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
                await _pushMessageService.PushMessageAsync(token, channel, _CammalSer(nevent), true);
        }
    }
}
