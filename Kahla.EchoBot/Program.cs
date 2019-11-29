using Kahla.EchoBot.Bot;
using Kahla.EchoBot.Core;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Kahla.EchoBot
{
    public class Program
    {
        static void Main(string[] args)
        {
            MainAsync().Wait();
        }

        internal static async Task MainAsync()
        {
            Console.WriteLine("Starting Kahla example bot...");

            // configure services.
            var scope = StartUp.ConfigureServices();

            // Get the bot.
            var botListener = scope.ServiceProvider.GetService<BotListener>();
            var commander = scope.ServiceProvider.GetService<BotCommander>();

            // Start bot.
            var bot = new EchoBotCore();
            var listenTask = await botListener
                .WithBot(bot)
                .Start();

            Console.WriteLine("Bot started. Waitting for commands.");
            await Task.WhenAll(listenTask, commander.Command());
        }
    }
}
