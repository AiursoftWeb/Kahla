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
using Microsoft.EntityFrameworkCore;
using WebPush;
using Microsoft.Extensions.Configuration;

namespace Kahla.Server.Services
{
    public class KahlaPushService
    {
        private readonly KahlaDbContext _dbContext;
        private readonly PushMessageService _stargatePushService;
        private readonly AppsContainer _appsContainer;
        private readonly ChannelService _channelService;
        private readonly IConfiguration _configuration;
        private readonly ThirdPartyPushService _thirdPartyPushService;

        public KahlaPushService(
            KahlaDbContext dbContext,
            PushMessageService stargatePushService,
            AppsContainer appsContainer,
            ChannelService channelService,
            IConfiguration configuration,
            ThirdPartyPushService thirdPartyPushService)
        {
            _dbContext = dbContext;
            _stargatePushService = stargatePushService;
            _appsContainer = appsContainer;
            _channelService = channelService;
            _configuration = configuration;
            _thirdPartyPushService = thirdPartyPushService;
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

        public async Task NewMessageEvent(string recieverId, int conversationId, string content, KahlaUser sender, string aesKey, bool muted = false, List<KahlaUser> usersTable = null)
        {
            var token = await _appsContainer.AccessToken();
            KahlaUser targetUser = null;
            if (usersTable == null)
            {
                targetUser = await _dbContext.Users.AsNoTracking().SingleOrDefaultAsync(t => t.Id == recieverId);
            }
            else
            {
                targetUser = usersTable.SingleOrDefault(t => t.Id == recieverId);
            }
            var channel = targetUser.CurrentChannel;
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
            {
                await _stargatePushService.PushMessageAsync(token, channel, _CammalSer(nevent), true);
            }
            await _thirdPartyPushService.PushAsync(targetUser.Id, sender.Email, _CammalSer(nevent));
        }

        public async Task NewFriendRequestEvent(string recieverId, string requesterId)
        {
            var token = await _appsContainer.AccessToken();
            var reciever = await _dbContext.Users.FindAsync(recieverId);
            var requester = await _dbContext.Users.FindAsync(requesterId);
            var channel = reciever.CurrentChannel;
            var nevent = new NewFriendRequest
            {
                Type = EventType.NewFriendRequest,
                RequesterId = requesterId
            };
            if (channel != -1)
                await _stargatePushService.PushMessageAsync(token, channel, _CammalSer(nevent), true);
            await _thirdPartyPushService.PushAsync(reciever.Id, requester.Email, _CammalSer(nevent));
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
                await _stargatePushService.PushMessageAsync(token, channel, _CammalSer(nevent), true);
            await _thirdPartyPushService.PushAsync(user.Id, "postermaster@aiursoft.com", _CammalSer(nevent));
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
                await _stargatePushService.PushMessageAsync(token, channel, _CammalSer(nevent), true);
            await _thirdPartyPushService.PushAsync(user.Id, "postermaster@aiursoft.com", _CammalSer(nevent));
        }
    }
}
