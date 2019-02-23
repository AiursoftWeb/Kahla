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
    public class PushKahlaMessageService
    {
        private readonly KahlaDbContext _dbContext;
        private readonly PushMessageService _pushMessageService;
        private readonly AppsContainer _appsContainer;
        private readonly ChannelService _channelService;
        private readonly IConfiguration _configuration;

        public PushKahlaMessageService(
            KahlaDbContext dbContext,
            PushMessageService pushMessageService,
            AppsContainer appsContainer,
            ChannelService channelService,
            IConfiguration configuration)
        {
            _dbContext = dbContext;
            _pushMessageService = pushMessageService;
            _appsContainer = appsContainer;
            _channelService = channelService;
            _configuration = configuration;
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
            if(usersTable == null)
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
            if (channel != -1) {
                await _pushMessageService.PushMessageAsync(token, channel, _CammalSer(nevent), true);

                var devices = _dbContext.Devices
                    .Where(m => m.UserID == recieverId)
                    .ToList();

                string vapidPublicKey = _configuration.GetSection("VapidKeys")["PublicKey"];
                string vapidPrivateKey = _configuration.GetSection("VapidKeys")["PrivateKey"];

                foreach (var device in devices)
                {
                    var pushSubscription = new PushSubscription(device.PushEndpoint, device.PushP256DH, device.PushAuth);
                    var vapidDetails = new VapidDetails("mailto:" + targetUser.Email, vapidPublicKey, vapidPrivateKey);

                    var webPushClient = new WebPushClient();
                    string payload = JsonConvert.SerializeObject(new {
                        title = sender.NickName,
                        message = content,
                        aesKey = aesKey});
                    try
                    {
                        webPushClient.SendNotification(pushSubscription, payload, vapidDetails);
                    }
                    catch (WebPushException exception)
                    {
                        Console.WriteLine(exception);
                    }
                }

            }
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
