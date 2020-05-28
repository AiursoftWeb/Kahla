using Kahla.SDK.Abstract;
using Kahla.SDK.Data;
using Kahla.SDK.Services;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.SDK.CommandHandlers
{
    public class SayCommandHandler<T> : ICommandHandler where T : BotBase
    {
        private readonly ConversationService _conversationService;
        private readonly BotLogger _botLogger;
        private readonly EventSyncer _eventSyncer;
        private readonly AES _aes;

        public SayCommandHandler(
            ConversationService conversationService,
            BotLogger botLogger,
            EventSyncer eventSyncer,
            AES aes)
        {
            _conversationService = conversationService;
            _botLogger = botLogger;
            _eventSyncer = eventSyncer;
            _aes = aes;
        }

        public  bool CanHandle(string command)
        {
            return command.StartsWith("say");
        }

        public async  Task Execute(string command)
        {
            var conversations = await _conversationService.AllAsync();
            _botLogger.LogInfo($"");
            foreach (var conversation in conversations.Items)
            {
                _botLogger.LogInfo($"ID: {conversation.ConversationId}\tName:\t{conversation.DisplayName}");
            }
            _botLogger.LogInfo($"");
            var convId = _botLogger.ReadLine($"Enter conversation ID you want to say:");
            var target = conversations.Items.FirstOrDefault(t => t.ConversationId.ToString() == convId);
            if (target == null)
            {
                _botLogger.LogDanger($"Can't find conversation with ID: {convId}");
                return;
            }
            var toSay = _botLogger.ReadLine($"Enter the message you want to send to '{target.DisplayName}':");
            if (string.IsNullOrWhiteSpace(toSay))
            {
                _botLogger.LogDanger($"Can't send empty content.");
                return;
            }
            var encrypted = _aes.OpenSSLEncrypt(toSay, _eventSyncer.Contacts.FirstOrDefault(t => t.ConversationId == target.ConversationId)?.AesKey);
            await _conversationService.SendMessageAsync(encrypted, target.ConversationId);
            _botLogger.LogSuccess($"Sent.");
        }
    }
}
