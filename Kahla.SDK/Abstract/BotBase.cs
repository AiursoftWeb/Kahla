using Kahla.Bot.Services;
using Kahla.SDK.Core;
using Kahla.SDK.Events;
using Kahla.SDK.Models;
using Kahla.SDK.Services;
using System.Threading.Tasks;

namespace Kahla.SDK.Abstract
{
    public abstract class BotBase
    {
        public AES AES;
        public BotListener BotListener;
        public BotCommander BotCommander;
        public BotLogger BotLogger;
        public ConversationService ConversationService;

        public abstract KahlaUser Profile { get; set; }

        public abstract Task<bool> OnFriendRequest(NewFriendRequestEvent arg);

        public abstract Task OnMessage(string inputMessage, NewMessageEvent eventContext);

        public virtual async Task Start()
        {
            var listenTask = await BotListener
                .Start();

            BotLogger.LogSuccess("Bot started! Waitting for commands. Enter 'help' to view available commands.");
            await Task.WhenAll(listenTask, BotCommander.Command());
        }

        public virtual async Task SendMessage(string message, int conversationId, string aesKey)
        {
            var encrypted = AES.OpenSSLEncrypt(message, aesKey);
            await ConversationService.SendMessageAsync(encrypted, conversationId);
        }
    }
}
