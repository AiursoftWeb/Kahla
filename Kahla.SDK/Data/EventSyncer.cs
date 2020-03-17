using Aiursoft.Scanner.Interfaces;
using Kahla.SDK.Abstract;
using Kahla.SDK.Events;
using Kahla.SDK.Models.ApiViewModels;
using Kahla.SDK.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Websocket.Client;

namespace Kahla.SDK.Data
{
    public class EventSyncer : ISingletonDependency
    {
        private readonly ConversationService _conversationService;
        private readonly BotLogger _botLogger;
        private readonly AES _aes;

        private WebsocketClient _websocket;
        private BotBase _bot;

        public List<ContactInfo> Contacts { get; set; }

        public EventSyncer(
            ConversationService conversationService,
            BotLogger botLogger,
            AES aes)
        {
            _conversationService = conversationService;
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
                await _bot.OnFriendRequest(typedEvent);
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
    }
}
