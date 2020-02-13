using Kahla.SDK.Abstract;
using Kahla.SDK.Events;
using Kahla.SDK.Models.ApiViewModels;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Kahla.Bot.Bots
{
    public class EchoBot : BotBase
    {
        public override Task OnBotInit()
        {
            var profilestring = JsonConvert.SerializeObject(Profile, Formatting.Indented);
            BotLogger.LogVerbose(profilestring);
            return Task.CompletedTask;
        }

        public override Task OnFriendRequest(NewFriendRequestEvent arg)
        {
            return CompleteRequest(arg.RequestId, true);
        }

        public override async Task OnGroupInvitation(int groupId, NewMessageEvent eventContext)
        {
            var group = await GroupsService.GroupSummaryAsync(groupId);
            if (!group.Value.HasPassword)
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
                replaced = AddMention(replaced, eventContext.Message.Sender);
            }
            await Task.Delay(700);
            await SendMessage(replaced, eventContext.Message.ConversationId, eventContext.AESKey);
        }
    }
}
