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
            _bot = bot;
            await SyncFromServer();
            client.MessageReceived.Subscribe(OnStargateMessage);
        }

        public async Task SyncFromServer()
        {
            var allResponse = await _conversationService.AllAsync();
            Contacts = allResponse.Items;
            foreach (var contact in Contacts)
            {
                if (contact.LatestMessage != null)
                {
                    contact.Messages.Add(contact.LatestMessage);
                }
            }

            var requestsResponse = await _friendshipService.MyRequestsAsync();
            Requests = requestsResponse.Items;
        }

        public async void OnStargateMessage(ResponseMessage msg)
        {
            var inevent = JsonConvert.DeserializeObject<KahlaEvent>(msg.ToString());
            switch (inevent.Type)
            {
                case EventType.NewMessage:
                    var newMessageEvent = JsonConvert.DeserializeObject<NewMessageEvent>(msg.ToString());
                    await InsertNewMessage(
                        newMessageEvent.ConversationId,
                        newMessageEvent.Message,
                        newMessageEvent.Mentioned,
                        newMessageEvent.PreviousMessageId);
                    await OnNewMessageEvent(newMessageEvent);
                    break;
                case EventType.NewFriendRequestEvent:
                    var newFriendRequestEvent = JsonConvert.DeserializeObject<NewFriendRequestEvent>(msg.ToString());
                    PatchFriendRequest(newFriendRequestEvent.Request);
                    await _bot.OnFriendRequest(newFriendRequestEvent);
                    break;
                case EventType.FriendsChangedEvent:
                    var friendsChangedEvent = JsonConvert.DeserializeObject<FriendsChangedEvent>(msg.ToString());
                    PatchFriendRequest(friendsChangedEvent.Request);
                    if (friendsChangedEvent.Result)
                    {
                        SyncFriendRequestToContacts(friendsChangedEvent.Request, friendsChangedEvent.CreatedConversation);
                    }
                    await _bot.OnFriendsChangedEvent(friendsChangedEvent);
                    break;
                case EventType.FriendDeletedEvent:
                    var friendDeletedEvent = JsonConvert.DeserializeObject<FriendDeletedEvent>(msg.ToString());
                    DeleteConversationIfExist(friendDeletedEvent.ConversationId);
                    await _bot.OnWasDeleted(friendDeletedEvent);
                    break;
                case EventType.DissolveEvent:
                    var dissolveEvent = JsonConvert.DeserializeObject<DissolveEvent>(msg.ToString());
                    DeleteConversationIfExist(dissolveEvent.ConversationId);
                    await _bot.OnGroupDissolve(dissolveEvent);
                    break;
                case EventType.SomeoneLeftEvent:
                    var someoneLeftEvent = JsonConvert.DeserializeObject<SomeoneLeftEvent>(msg.ToString());
                    if (someoneLeftEvent.LeftUser.Id == _bot.Profile.Id)
                    {
                        // you was kicked
                        DeleteConversationIfExist(someoneLeftEvent.ConversationId);
                    }
                    else
                    {
                        // Some other one, not me, was deleted in a conversation.
                    }
                    break;
                case EventType.GroupJoinedEvent:
                    var groupJoinedEvent = JsonConvert.DeserializeObject<GroupJoinedEvent>(msg.ToString());
                    SyncGroupToContacts(groupJoinedEvent.CreatedConversation, groupJoinedEvent.MessageCount, groupJoinedEvent.LatestMessage);
                    await _bot.OnGroupConnected(new SearchedGroup(groupJoinedEvent.CreatedConversation));
                    break;
                default:
                    _botLogger.LogDanger($"Unhandled server event: {inevent.TypeDescription}!");
                    break;
            }
        }

        protected virtual async Task OnNewMessageEvent(NewMessageEvent typedEvent)
        {
            string decrypted = _aes.OpenSSLDecrypt(typedEvent.Message.Content, typedEvent.AESKey);
            _botLogger.LogInfo($"On message from sender `{typedEvent.Message.Sender.NickName}`: {decrypted}");
            if (decrypted.StartsWith("[group]") && int.TryParse(decrypted.Substring(7), out int groupId))
            {
                await _bot.OnGroupInvitation(groupId, typedEvent);
            }
            await _bot.OnMessage(decrypted, typedEvent).ConfigureAwait(false);
        }

        private void PatchFriendRequest(Request request)
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

        private async Task InsertNewMessage(int conversationId, Message message, bool mentioned, string previousMessageId)
        {
            if (!Contacts.Any(t => t.ConversationId == conversationId))
            {
                _botLogger.LogDanger($"Comming new message from conversation: '{conversationId}' but we can't find it in memory.");
                return;
            }
            var conversation = Contacts.SingleOrDefault(t => t.ConversationId == conversationId);
            if (Guid.Parse(previousMessageId) != Guid.Empty)  // On server, has previous message.)
            {
                if (conversation.LatestMessage.Id != Guid.Parse(previousMessageId) || // Local latest message is not latest.
                   !conversation.Messages.Any(t => t.Id == Guid.Parse(previousMessageId))) // Server side previous message do not exists locally.
                {
                    // Some message was lost.
                    _botLogger.LogWarning($"Some message was lost. Trying to sync...");
                    var missedMessages = await _conversationService.GetMessagesAsync(conversationId, 15, message.Id.ToString());
                    foreach (var missedMessage in missedMessages.Items)
                    {
                        if (!conversation.Messages.Any(t => t.Id == missedMessage.Id))
                        {
                            conversation.Messages.Add(missedMessage);
                        }

                    }
                }
            }
            conversation.LatestMessage = message;
            conversation.Messages.Add(message);
        }

        private void SyncFriendRequestToContacts(Request request, PrivateConversation createdConversation)
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
                    request.Target.IsOnline
            }); ;
        }

        private void SyncGroupToContacts(GroupConversation createdConversation, int messageCount, Message latestMessage)
        {
            Contacts.Add(new ContactInfo
            {
                AesKey = createdConversation.AESKey,
                SomeoneAtMe = false,
                UnReadAmount = messageCount,
                ConversationId = createdConversation.Id,
                Discriminator = nameof(GroupConversation),
                DisplayImagePath = createdConversation.GroupImagePath,
                DisplayName = createdConversation.GroupName,
                EnableInvisiable = false,
                LatestMessage = latestMessage,
                Muted = false,
                Online = false,
                UserId = createdConversation.OwnerId
            });
        }

        private void DeleteConversationIfExist(int conversationId)
        {
            if (Contacts.Any(t => t.ConversationId == conversationId))
            {
                Contacts.RemoveAll(t => t.ConversationId == conversationId);
            }
        }
    }
}
