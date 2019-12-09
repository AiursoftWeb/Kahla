using Kahla.Bot.Core;
using Kahla.Bot.Services;
using Kahla.SDK.Events;
using Kahla.SDK.Models;
using System.Threading.Tasks;

namespace Kahla.Bot.Abstract
{
    public abstract class BotBase
    {
        public BotListener BotListener;
        public BotCommander BotCommander;
        public BotLogger BotLogger;

        public abstract KahlaUser Profile { get; set; }

        public abstract Task<bool> OnFriendRequest(NewFriendRequestEvent arg);

        public abstract Task<string> OnMessage(string inputMessage, NewMessageEvent eventContext);

        public virtual async Task Start()
        {
            var listenTask = await BotListener
                .Start();

            BotLogger.LogSuccess("Bot started! Waitting for commands. Enter 'help' to view available commands.");
            await Task.WhenAll(listenTask, BotCommander.Command());
        }
    }
}
