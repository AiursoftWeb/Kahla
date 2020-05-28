using Aiursoft.Handler.Exceptions;
using Aiursoft.Handler.Models;
using Aiursoft.Probe.SDK.Models.FilesViewModels;
using Aiursoft.XelNaga.Tools;
using Kahla.SDK.Data;
using Kahla.SDK.Events;
using Kahla.SDK.Models;
using Kahla.SDK.Models.ApiViewModels;
using Kahla.SDK.Services;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Websocket.Client;

namespace Kahla.SDK.Abstract
{
    public abstract class BotBase
    {
        public AES AES;
        public BotLogger BotLogger;
        public GroupsService GroupsService;
        public ConversationService ConversationService;
        public FriendshipService FriendshipService;
        public AuthService AuthService;
        public HomeService HomeService;
        public KahlaLocation KahlaLocation;
        public VersionService VersionService;
        public SettingsService SettingsService;
        public StorageService StorageService;

        public KahlaUser Profile { get; set; }
        public IEnumerable<ContactInfo> Contacts { get; set; }

        public virtual Task OnBotStarting()
        {
            BotLogger.LogInfo("Bot Starting...");
            return Task.CompletedTask;
        }
        public virtual Task OnBotStarted()
        {
            var profilestring = JsonConvert.SerializeObject(Profile, Formatting.Indented);
            BotLogger.LogVerbose(profilestring);
            return Task.CompletedTask;
        }

        public virtual Task OnFriendRequest(NewFriendRequestEvent arg) => Task.CompletedTask;

        public virtual Task OnFriendsChangedEvent(FriendsChangedEvent arg) => Task.CompletedTask;

        public virtual Task OnGroupConnected(SearchedGroup group) => Task.CompletedTask;

        public virtual Task OnMessage(string inputMessage, NewMessageEvent eventContext) => Task.CompletedTask;

        public virtual Task OnGroupInvitation(int groupId, NewMessageEvent eventContext) => Task.CompletedTask;

        public virtual Task OnWasDeleted(FriendDeletedEvent typedEvent) => Task.CompletedTask;

        public virtual Task OnGroupDissolve(DissolveEvent typedEvent) => Task.CompletedTask;

        public virtual Task OnMemoryChanged() => Task.CompletedTask;

        public async Task<int> CompleteRequest(int requestId, bool accept)
        {
            var text = accept ? "accepted" : "rejected";
            BotLogger.LogWarning($"Friend request with id '{requestId}' was {text}.");
            var createdConversationId = await FriendshipService.CompleteRequestAsync(requestId, accept);
            return createdConversationId.Value;
        }

        public Task MuteGroup(string groupName, bool mute)
        {
            var text = mute ? "muted" : "unmuted";
            BotLogger.LogWarning($"Group with name '{groupName}' was {text}.");
            return GroupsService.SetGroupMutedAsync(groupName, mute);
        }

        private async Task SendFileWithPattern(int conversationId, Stream file, string fileName, string pattern)
        {
            var token = await StorageService.InitFileAccessAsync(conversationId, true, false);
            var fileResponse = await StorageService.Http.PostWithFile(token.UploadAddress, file, fileName);
            var fileResponseObject = JsonConvert.DeserializeObject<UploadFileViewModel>(fileResponse);
            await SendMessage(string.Format(pattern, fileResponseObject.FilePath), conversationId);
        }

        public Task SendPhoto(int conversationId, Stream file, string fileName)
        {
            var image = Image.Load(file, out _);
            return SendFileWithPattern(conversationId, file, fileName, "[img]{0}|" + $"{image.Width}|{image.Height}");
        }

        public Task SendFile(int conversationId, Stream file, string fileName)
        {
            return SendFileWithPattern(conversationId, file, fileName, "[file]{0}|" + $"{fileName}|{file.Length.HumanReadableSize()}");
        }

        public Task SendVideo(int conversationId, Stream file, string fileName)
        {
            return SendFileWithPattern(conversationId, file, fileName, "[video]{0}");
        }

        public Task SendAudio(int conversationId, Stream file, string fileName)
        {
            return SendFileWithPattern(conversationId, file, fileName, "[audio]{0}");
        }

        public Task SendGroupCard(int conversationId, int groupConversationId)
        {
            return SendMessage($"[group]{groupConversationId}", conversationId);
        }

        public Task SendUserCard(int conversationId, string userId)
        {
            return SendMessage($"[user]{userId}", conversationId);
        }

        public async Task SendMessage(string message, int conversationId)
        {
            var encrypted = AES.OpenSSLEncrypt(message, Contacts.FirstOrDefault(t => t.ConversationId == conversationId)?.AesKey);
            await ConversationService.SendMessageAsync(encrypted, conversationId);
        }

        public async Task JoinGroup(string groupName, string password)
        {
            try
            {
                var result = await GroupsService.JoinGroupAsync(groupName, password);
                var group = await GroupsService.GroupSummaryAsync(result.Value);
                await OnGroupConnected(group.Value);
            }
            catch (AiurUnexceptedResponse e) when (e.Code == ErrorType.HasDoneAlready)
            {
                // do nothing.
            }
        }

        protected string RemoveMentionMe(string sourceMessage)
        {
            sourceMessage = sourceMessage.Replace($"@{Profile.NickName.Replace(" ", "")}", "");
            return sourceMessage;
        }

        protected string Mention(KahlaUser target)
        {
            return $" @{target.NickName.Replace(" ", "")}";
        }
    }
}
