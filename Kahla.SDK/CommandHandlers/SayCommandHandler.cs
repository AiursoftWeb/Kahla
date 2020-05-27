using Kahla.SDK.Abstract;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.SDK.CommandHandlers
{
    [CommandHandler("say")]
    public class SayCommandHandler<T> : CommandHandlerBase<T> where T : BotBase
    {
        public SayCommandHandler(BotCommander<T> botCommander) : base(botCommander)
        {
        }

        public async override Task Execute(string command)
        {
            var conversations = await _botCommander._conversationService.AllAsync();
            _botCommander._botLogger.LogInfo($"");
            foreach (var conversation in conversations.Items)
            {
                _botCommander._botLogger.LogInfo($"ID: {conversation.ConversationId}\tName:\t{conversation.DisplayName}");
            }
            _botCommander._botLogger.LogInfo($"");
            var convId = _botCommander._botLogger.ReadLine($"Enter conversation ID you want to say:");
            var target = conversations.Items.FirstOrDefault(t => t.ConversationId.ToString() == convId);
            if (target == null)
            {
                _botCommander._botLogger.LogDanger($"Can't find conversation with ID: {convId}");
                return;
            }
            var toSay = _botCommander._botLogger.ReadLine($"Enter the message you want to send to '{target.DisplayName}':");
            if (string.IsNullOrWhiteSpace(toSay))
            {
                _botCommander._botLogger.LogDanger($"Can't send empty content.");
                return;
            }
            await _botCommander._botHost._bot.SendMessage(toSay, target.ConversationId);
            _botCommander._botLogger.LogSuccess($"Sent.");
        }
    }
}
