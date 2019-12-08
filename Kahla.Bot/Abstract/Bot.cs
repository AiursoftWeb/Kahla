using Kahla.Bot.Core;
using Kahla.Bot.Services;
using Kahla.SDK.Events;
using Kahla.SDK.Models;
using System.Threading.Tasks;

namespace Kahla.Bot.Abstract
{
    public abstract class BotBase
    {
        protected readonly BotListener BotListener;
        protected readonly BotCommander BotCommander;
        private readonly BotLogger _botLogger;

        public BotBase(
            BotListener botListener,
            BotCommander botCommander,
            BotLogger botLogger)
        {
            BotListener = botListener.WithBot(this);
            BotCommander = botCommander.WithBot(this);
            _botLogger = botLogger;
        }

        public abstract KahlaUser Profile { get; set; }

        public abstract Task<bool> OnFriendRequest(NewFriendRequestEvent arg);

        public abstract Task<string> OnMessage(string inputMessage, NewMessageEvent eventContext);

        public virtual async Task Start()
        {
            var listenTask = await BotListener
                .Start();

            _botLogger.LogSuccess("Bot started! Waitting for commands. Enter 'help' to view available commands.");
            await Task.WhenAll(listenTask, BotCommander.Command());
        }
    }
}
