using Kahla.SDK.Abstract;
using System.Threading.Tasks;

namespace Kahla.SDK.CommandHandlers
{
    [CommandHandler("conv")]
    public class ConvCommandHandler : CommandHandlerBase
    {
        public ConvCommandHandler(BotCommander botCommander) : base(botCommander)
        {
        }

        public async override Task Execute(string command)
        {
            var conversations = await _botCommander._conversationService.AllAsync();
            _botCommander._botLogger.LogSuccess($"Successfully get all your conversations.");
            foreach (var conversation in conversations.Items)
            {
                var online = conversation.Online ? "online" : "offline";
                _botCommander._botLogger.LogInfo($"Name:\t{conversation.DisplayName}");
                _botCommander._botLogger.LogInfo($"ID:\t{conversation.ConversationId}\t{online}\t\t{conversation.Discriminator}");
                if (!string.IsNullOrWhiteSpace(conversation.LatestMessage))
                {
                    _botCommander._botLogger.LogInfo($"Last:\t{_botCommander._aes.OpenSSLDecrypt(conversation.LatestMessage, conversation.AesKey)}");
                    _botCommander._botLogger.LogInfo($"Time:\t{conversation.LatestMessageTime}");
                }
                if (conversation.UnReadAmount > 0)
                {
                    _botCommander._botLogger.LogDanger($"Unread:\t**{conversation.UnReadAmount}**");
                }
                else
                {
                    _botCommander._botLogger.LogInfo($"Unread:\t{conversation.UnReadAmount}");
                }
                if (conversation.SomeoneAtMe)
                {
                    _botCommander._botLogger.LogWarning($"At!");
                }
                _botCommander._botLogger.LogInfo($"\n");
            }
        }
    }
}
