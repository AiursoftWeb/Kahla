using Kahla.EchoBot.Bot;
using Kahla.EchoBot.Core;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Kahla.EchoBot
{
    public class Program
    {
        static void Main(string[] args)
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
            bot.Start().Wait();

            Console.WriteLine("Bot started. Waitting for commands.");
            commander.Command();
        }
    }
}
