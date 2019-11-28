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
            var bot = scope.ServiceProvider.GetService<BotListener>();
            var commander = scope.ServiceProvider.GetService<BotCommander>();

            // Give the bot logic.
            var botLogic = new EchoBotCore();
            bot.OnGetProfile = botLogic.SetProfile;
            bot.GenerateResponse = botLogic.ResponseUserMessage;
            bot.GenerateFriendRequestResult = botLogic.ResponseFriendRequest;

            // Start bot.
            var waittask = await bot.Start();

            Console.WriteLine("Bot started. Waitting for commands.");
            await Task.WhenAll(waittask, Task.Run(commander.Command));
        }
    }
}
