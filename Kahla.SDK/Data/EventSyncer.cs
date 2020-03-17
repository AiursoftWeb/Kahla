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
            BotBase bot)
        {
            _websocket = client;
            _bot = bot;
            await Clone();
            client.MessageReceived.Subscribe(OnStargateMessage);
        }

        public async Task Clone()
        {
            var allResponse = await _conversationService.AllAsync();
            Contacts = allResponse.Items;

            var requestsResponse = await _friendshipService.MyRequestsAsync();
            Requests = requestsResponse.Items;
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
                PatchFriendRequest(typedEvent.Request);
                await _bot.OnFriendRequest(typedEvent);
            }
            else if (inevent.Type == EventType.FriendsChangedEvent)
            {
                var typedEvent = JsonConvert.DeserializeObject<FriendsChangedEvent>(msg.ToString());
                PatchFriendRequest(typedEvent.Request);
                if (typedEvent.Result)
                {
                    SyncFriendRequestToContacts(typedEvent.Request, typedEvent.CreatedConversation);
                }
                await _bot.OnFriendsChangedEvent(typedEvent);
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

        public void PatchFriendRequest(Request request)
        {
            if (request.TargetId == _bot.Profile.Id)
            {
                // Sent to me from another user.
                var inMemory = Requests.SingleOrDefault(t => t.Id == request.Id);
                if (inMemory == null)
                {
                    Requests.Add(request);
                }
                else
                {
                    inMemory = request;
                }
            }
        }

        public void SyncFriendRequestToContacts(
            Request request,
            PrivateConversation createdConversation)
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
