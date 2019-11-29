using Kahla.Bot.Bots;
using Kahla.Bot.Core;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Kahla.Bot
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Kahla example bot...");
            MainAsync().Wait();
        }

        internal static async Task MainAsync()
        {
            // configure services.
            var scope = StartUp.ConfigureServices();

            // Get the bot.
            var botListener = scope.ServiceProvider.GetService<BotListener>();
            var commander = scope.ServiceProvider.GetService<BotCommander>();

            // Start bot.
            var bot = new EchoBot();
            var listenTask = await botListener
                .WithBot(bot)
                .Start();

            Console.WriteLine("Bot started. Waitting for commands.");
            await Task.WhenAll(listenTask, commander.Command());
        }
    }
}
