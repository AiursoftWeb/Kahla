using Kahla.SDK.Abstract;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.SDK.CommandHandlers
{
    [CommandHandler("conv")]
    public class ConvCommandHandler<T> : CommandHandlerBase<T> where T : BotBase
    {
        public ConvCommandHandler(BotCommander<T> botCommander) : base(botCommander)
        {
        }

        public async override Task Execute(string command)
        {
            await Task.Delay(0);
            if (int.TryParse(command.Substring(4).Trim(), out int convId))
            {
                var conversation = _botCommander._botHost._bot.EventSyncer.Contacts.FirstOrDefault(t => t.ConversationId == convId);
                if (conversation == null)
                {
                    _botCommander._botLogger.LogDanger($"Conversation with Id '{convId}' was not found!");
                }
                foreach (var message in conversation.Messages)
                {
                    if (!message.GroupWithPrevious)
                    {
                        _botCommander._botLogger.LogInfo($"{message.Sender.NickName} says: \t {_botCommander._aes.OpenSSLDecrypt(message.Content, conversation.AesKey)}");
                    }
                    else
                    {
                        _botCommander._botLogger.LogInfo($"\t\t\t {_botCommander._aes.OpenSSLDecrypt(message.Content, conversation.AesKey)}");
                    }
                }
                return;
            }
            foreach (var conversation in _botCommander._botHost._bot.EventSyncer.Contacts)
            {
                var online = conversation.Online == true ? "online" :
                             conversation.Online == false ? "offline" : string.Empty;
                _botCommander._botLogger.LogInfo($"Name:\t{conversation.DisplayName}");
                _botCommander._botLogger.LogInfo($"ID:\t{conversation.ConversationId}\t{online}\t\t{conversation.Discriminator}");
                if (!string.IsNullOrWhiteSpace(conversation.LatestMessage?.Content))
                {
                    _botCommander._botLogger.LogInfo($"Last:\t{_botCommander._aes.OpenSSLDecrypt(conversation.LatestMessage.Content, conversation.AesKey)}");
                    _botCommander._botLogger.LogInfo($"Time:\t{conversation.LatestMessage.SendTime}");
                }
                if (conversation.UnReadAmount > 0)
                {
                    _botCommander._botLogger.LogDanger($"Unread:\t**{conversation.UnReadAmount}**");
                }
                if (conversation.SomeoneAtMe)
                {
                    _botCommander._botLogger.LogWarning($"At!");
                }
                if (conversation.Messages.Count > 0)
                {
                    _botCommander._botLogger.LogVerbose($"Local Messages:\t**{conversation.Messages.Count}**");
                }
                _botCommander._botLogger.LogInfo($"\n");
            }
        }
    }
}
