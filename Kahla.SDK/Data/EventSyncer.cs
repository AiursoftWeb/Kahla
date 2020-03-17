using Aiursoft.Scanner.Interfaces;
using Kahla.SDK.Abstract;
using Kahla.SDK.Events;
using Kahla.SDK.Models;
using Kahla.SDK.Models.ApiViewModels;
using Kahla.SDK.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Websocket.Client;

namespace Kahla.SDK.Data
{
    public class EventSyncer : ISingletonDependency
    {
        private readonly ConversationService _conversationService;
        private readonly FriendshipService _friendshipService;
        private readonly BotLogger _botLogger;
        private readonly AES _aes;

        private WebsocketClient _websocket;
        private BotBase _bot;

        public List<ContactInfo> Contacts { get; set; }
        public List<Request> Requests { get; set; }

        public EventSyncer(
            ConversationService conversationService,
            FriendshipService friendshipService,
            BotLogger botLogger,
            AES aes)
        {
            _conversationService = conversationService;
            _friendshipService = friendshipService;
            _botLogger = botLogger;
            _aes = aes;
        }

        public async Task Init(
            WebsocketClient client,
            BotBase bot,
            bool forceRefresh = false)
        {
            _websocket = client;
            _bot = bot;
            if (Contacts == null || forceRefresh)
            {
                var allResponse = await _conversationService.AllAsync();
                Contacts = allResponse.Items;
            }
            if (Requests == null || forceRefresh)
            {
                var requestsResponse = await _friendshipService.MyRequestsAsync();
                Requests = requestsResponse.Items;
            }
            client.MessageReceived.Subscribe(OnStargateMessage);
        }

        public async void OnStargateMessage(ResponseMessage msg)
        {
            var inevent = JsonConvert.DeserializeObject<KahlaEvent>(msg.ToString());
            if (inevent.Type == EventType.NewMessage)
            {
                var typedEvent = JsonConvert.DeserializeObject<NewMessageEvent>(msg.ToString());
                await OnNewMessageEvent(typedEvent);
            }
            else if (inevent.Type == EventType.NewFriendRequestEvent)
            {
                var typedEvent = JsonConvert.DeserializeObject<NewFriendRequestEvent>(msg.ToString());
                if (!Requests.Any(t => t.Id == typedEvent.Request.Id))
                {
                    Requests.Add(typedEvent.Request);
                }
                await _bot.OnFriendRequest(typedEvent);
            }
            else if (inevent.Type == EventType.FriendsChangedEvent)
            {
                var typedEvent = JsonConvert.DeserializeObject<FriendsChangedEvent>(msg.ToString());
                SyncFriendRequest(typedEvent.Request, typedEvent.Result, typedEvent.CreatedConversation);
            }
        }

        public async Task OnNewMessageEvent(NewMessageEvent typedEvent)
        {
            string decrypted = _aes.OpenSSLDecrypt(typedEvent.Message.Content, typedEvent.AESKey);
            _botLogger.LogInfo($"On message from sender `{typedEvent.Message.Sender.NickName}`: {decrypted}");
            if (decrypted.StartsWith("[group]") && int.TryParse(decrypted.Substring(7), out int groupId))
            {
                await _bot.OnGroupInvitation(groupId, typedEvent);
            }
            else
            {
                await _bot.OnMessage(decrypted, typedEvent).ConfigureAwait(false);
            }
        }

        public void SyncFriendRequest(
            Request request,
            bool accept,
            PrivateConversation createdConversation)
        {
            if (request.TargetId == _bot.Profile.Id)
            {
                // Sent to me from another user.
                var inMemory = Requests.SingleOrDefault(t => t.Id == request.Id);
                inMemory = request;
            }
            if (accept)
            {
                Contacts.Add(new ContactInfo
                {
                    DisplayName = request.TargetId == _bot.Profile.Id ?
                        request.Creator.NickName :
                        request.Target.NickName,
                    DisplayImagePath = request.TargetId == _bot.Profile.Id ?
                        request.Creator.IconFilePath :
                        request.Target.IconFilePath,
                    LatestMessage = null,
                    LatestMessageTime = DateTime.MinValue,
                    UnReadAmount = 0,
                    ConversationId = createdConversation.Id,
                    Discriminator = createdConversation.Discriminator,
                    UserId = request.TargetId == _bot.Profile.Id ?
                        request.Creator.Id :
                        request.Target.Id,
                    AesKey = createdConversation.AESKey,
                    Muted = createdConversation.Muted(_bot.Profile.Id),
                    SomeoneAtMe = false,
                    Online = request.TargetId == _bot.Profile.Id ?
                        request.Creator.IsOnline :
                        request.Target.IsOnline,
                    LatestMessageId = Guid.Empty
                }); ;
            }
        }
    }
}
