using Kahla.SDK.Abstract;
using Kahla.SDK.Data;
using Kahla.SDK.Services;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.SDK.CommandHandlers
{
    public class ConvCommandHandler<T> : ICommandHandler<T> where T : BotBase
    {
        private readonly AES _aes;
        private readonly EventSyncer _eventSyncer;
        private readonly BotLogger _botLogger;

        public ConvCommandHandler(
            AES aes,
            EventSyncer eventSyncer,
            BotLogger botLogger) 
        {
            _aes = aes;
            _eventSyncer = eventSyncer;
            _botLogger = botLogger;
        }

        public void InjectHost(BotHost<T> instance){ }
        public  bool CanHandle(string command)
        {
            return command.StartsWith("conv");
        }

        public async  Task Execute(string command)
        {
            await Task.Delay(0);
            if (int.TryParse(command.Substring(4).Trim(), out int convId))
            {
                var conversation = _eventSyncer.Contacts.FirstOrDefault(t => t.ConversationId == convId);
                if (conversation == null)
                {
                    _botLogger.LogDanger($"Conversation with Id '{convId}' was not found!");
                }
                foreach (var message in conversation.Messages)
                {
                    if (!message.GroupWithPrevious)
                    {
                        _botLogger.LogInfo($"{message.Sender.NickName} says: \t {_aes.OpenSSLDecrypt(message.Content, conversation.AesKey)}");
                    }
                    else
                    {
                        _botLogger.LogInfo($"\t\t\t {_aes.OpenSSLDecrypt(message.Content, conversation.AesKey)}");
                    }
                }
                return;
            }
            foreach (var conversation in _eventSyncer.Contacts)
            {
                var online = conversation.Online == true ? "online" :
                             conversation.Online == false ? "offline" : string.Empty;
                _botLogger.LogInfo($"Name:\t{conversation.DisplayName}");
                _botLogger.LogInfo($"ID:\t{conversation.ConversationId}\t{online}\t\t{conversation.Discriminator}");
                if (!string.IsNullOrWhiteSpace(conversation.LatestMessage?.Content))
                {
                    _botLogger.LogInfo($"Last:\t{_aes.OpenSSLDecrypt(conversation.LatestMessage.Content, conversation.AesKey)}");
                    _botLogger.LogInfo($"Time:\t{conversation.LatestMessage.SendTime}");
                }
                if (conversation.UnReadAmount > 0)
                {
                    _botLogger.LogDanger($"Unread:\t**{conversation.UnReadAmount}**");
                }
                if (conversation.SomeoneAtMe)
                {
                    _botLogger.LogWarning($"At!");
                }
                if (conversation.Messages.Count > 0)
                {
                    _botLogger.LogVerbose($"Local Messages:\t**{conversation.Messages.Count}**");
                }
                _botLogger.LogInfo($"\n");
            }
        }
    }
}
