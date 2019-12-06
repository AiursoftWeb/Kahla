using Aiursoft.Pylon;
using Aiursoft.Pylon.Interfaces;
using Kahla.Bot.Bots;
using Kahla.Bot.Core;
using Kahla.Bot.Services;
using Kahla.SDK.Models;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Threading.Tasks;

namespace Kahla.Bot
{
    public class StartUp : IScopedDependency
    {
        private readonly BotListener _botListener;
        private readonly BotCommander _botCommander;
        private readonly BotLogger _botLogger;
        private readonly EchoBot _echoBot;
        private readonly TranslateBot _translateBot;

        public StartUp(
            BotListener botListener,
            BotCommander botCommander,
            BotLogger botLogger,
            EchoBot bot,
            TranslateBot translateBot)
        {
            _botListener = botListener;
            _botCommander = botCommander;
            _botLogger = botLogger;
            _echoBot = bot;
            _translateBot = translateBot;
        }

        public static IServiceScope ConfigureServices()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };

            var services = new ServiceCollection();
            services.AddAiurDependencies<KahlaUser>("Kahla");
            Console.Clear();

            return services.BuildServiceProvider()
                 .GetService<IServiceScopeFactory>()
                 .CreateScope();
        }

        public async Task Start()
        {
            var listenTask = await _botListener
                .WithBot(_translateBot)
                .Start();

            _botLogger.LogSuccess("Bot started! Waitting for commands. Enter 'help' to view available commands.");
            await Task.WhenAll(listenTask, _botCommander.Command());
        }
    }
}
