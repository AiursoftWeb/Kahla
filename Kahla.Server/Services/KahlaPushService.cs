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
        private readonly AiurEmailSender _emailSender;

        public KahlaPushService(
            KahlaDbContext dbContext,
            PushMessageService stargatePushService,
            AppsContainer appsContainer,
            ChannelService channelService,
            ThirdPartyPushService thirdPartyPushService,
            AiurEmailSender emailSender)
        {
            _dbContext = dbContext;
            _stargatePushService = stargatePushService;
            _appsContainer = appsContainer;
            _channelService = channelService;
            _thirdPartyPushService = thirdPartyPushService;
            _emailSender = emailSender;
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
            var message = "";
            var newMessageEvent = new NewMessageEvent
            {
                ConversationId = conversation.Id,
                Sender = sender,
                Content = content,
                AESKey = conversation.AESKey,
                Muted = !alert
            };

            message = content;

            if (content.StartsWith("[img]"))
            {
                message = $"Click the link to open the picture: ${content}";
            }
            else if (content.StartsWith("[video]"))
            {
                message = $"Click the link to open the video: ${content}";
            }
            else if (content.StartsWith("[file]"))
            {
                message = $"Click the link to download the file: ${content}";
            }
            else if (content.StartsWith("[audio]"))
            {
                message = $"Click the link to open the audio: ${content}";
            }

            await _emailSender.SendEmail(receiver.Email, "There is a new message", $"Your friend {sender.NickName} sent you a new message {message}");

            var pushTasks = new List<Task>();
            if (channel != -1)
            {
                pushTasks.Add(_stargatePushService.PushMessageAsync(token, channel, _Serialize(newMessageEvent), true));
            }
            if (alert && receiver.Id != sender.Id)
            {
                pushTasks.Add(_thirdPartyPushService.PushAsync(receiver.Id, sender.Email, _Serialize(newMessageEvent)));
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

            await _emailSender.SendEmail(receiver.Email, "Add As a Friend", $"{requester.NickName} wants to be your friend");

            if (channel != -1)
            {
                await _stargatePushService.PushMessageAsync(token, channel, _Serialize(newFriendRequestEvent), true);
            }
            await _thirdPartyPushService.PushAsync(receiver.Id, requester.Email, _Serialize(newFriendRequestEvent));
        }

        public async Task WereDeletedEvent(KahlaUser receiver, KahlaUser trigger)
        {
            var token = await _appsContainer.AccessToken();
            var channel = receiver.CurrentChannel;
            var wereDeletedEvent = new WereDeletedEvent
            {
                Trigger = trigger
            };

            await _emailSender.SendEmail(receiver.Email, "Deleted you", $"Your friend {trigger.NickName} deleted you");

            if (channel != -1)
            {
                await _stargatePushService.PushMessageAsync(token, channel, _Serialize(wereDeletedEvent), true);
            }
            await _thirdPartyPushService.PushAsync(receiver.Id, "postermaster@aiursoft.com", _Serialize(wereDeletedEvent));
        }

        public async Task FriendAcceptedEvent(KahlaUser receiver, KahlaUser accepter)
        {
            var token = await _appsContainer.AccessToken();
            var channel = receiver.CurrentChannel;
            var friendAcceptedEvent = new FriendAcceptedEvent
            {
                Target = accepter
            };

            await _emailSender.SendEmail(receiver.Email, "Accpet with you", $"Your friend {accepter.NickName} agrees to your friend request");

            if (channel != -1)
            {
                await _stargatePushService.PushMessageAsync(token, channel, _Serialize(friendAcceptedEvent), true);
            }
            await _thirdPartyPushService.PushAsync(receiver.Id, "postermaster@aiursoft.com", _Serialize(friendAcceptedEvent));
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
                await _stargatePushService.PushMessageAsync(token, channel, _Serialize(timerUpdatedEvent), true);
            }
        }
    }
}
