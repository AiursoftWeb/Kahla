using Kahla.Bot.Services;
using Kahla.SDK.Abstract;
using Kahla.SDK.Events;
using Kahla.SDK.Models.ApiViewModels;

namespace Kahla.Bot.Bots
{
    public class TranslateBot : BotBase
    {
        private readonly BingTranslator _bingTranslator;
        public TranslateBot(BingTranslator bingTranslator)
        {
            _bingTranslator = bingTranslator;
        }

        public override Task OnBotStarting()
        {
            var key = SettingsService["BingTranslateAPIKey"] as string;
            if (string.IsNullOrWhiteSpace(key))
            {
                BotLogger.LogWarning("Please enter your bing API key:");
                key = Console.ReadLine();
            }
            SettingsService["BingTranslateAPIKey"] = key;
            return Task.CompletedTask;
        }

        public override Task OnFriendRequest(NewFriendRequestEvent context)
        {
            return CompleteRequest(context.Request.Id, true);
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
            if (eventContext.Muted)
            {
                return;
            }
            if (eventContext.Message.SenderId == Profile.Id)
            {
                return;
            }
            inputMessage = RemoveMentionMe(inputMessage);
            var translated = _bingTranslator.CallTranslate(inputMessage, "en");
            if (eventContext.Mentioned)
            {
                translated += Mention(eventContext.Message.Sender);
            }
            await SendMessage(translated, eventContext.ConversationId);
        }
    }
}
