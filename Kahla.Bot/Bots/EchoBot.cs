﻿using Kahla.SDK.Abstract;
using Kahla.SDK.Events;
using Kahla.SDK.Models;
using Kahla.SDK.Models.ApiViewModels;

namespace Kahla.Bot.Bots
{
    public class EchoBot : BotBase
    {
        public override Task OnFriendRequest(NewFriendRequestEvent context)
        {
            return CompleteRequest(context.Request.Id, true);
        }

        public override async Task OnGroupInvitation(int groupId, NewMessageEvent eventContext)
        {
            var group = await GroupsService.GroupSummaryAsync(groupId);
            if (!group.Value!.HasPassword)
            {
                await JoinGroup(group.Value.Name, string.Empty);
            }
        }

        public async override Task OnGroupConnected(SearchedGroup group)
        {
            await MuteGroup(group.Name, true);
        }

        public override async Task OnMessage(string inputMessage, NewMessageEvent eventContext)
        {
            await Task.Delay(0);
            if (eventContext.Muted)
            {
                return;
            }
            if (eventContext.Message.SenderId == Profile.Id)
            {
                return;
            }
            var replaced = RemoveMentionMe(inputMessage
                    .Replace("吗", "")
                    .Replace('？', '！')
                    .Replace('?', '!'));
            if (eventContext.Mentioned)
            {
                replaced += Mention(eventContext.Message.Sender);
            }
            await Task.Delay(700);
            await SendMessage(replaced, eventContext.ConversationId);
        }

        public async Task BroadcastAsync(string message)
        {
            await BroadcastMessage(message, c => c.Discriminator == nameof(PrivateConversation));
        }
    }
}
