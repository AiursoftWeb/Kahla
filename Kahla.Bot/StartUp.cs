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

        public StartUp(
            BotListener botListener,
            BotCommander botCommander,
            BotLogger botLogger,
            EchoBot echoBot)
        {
            _botListener = botListener;
            _botCommander = botCommander;
            _botLogger = botLogger;
            _echoBot = echoBot;
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

        public Task Start()
        {
            _echoBot.BotCommander = _botCommander.WithBot(_echoBot);
            _echoBot.BotListener = _botListener.WithBot(_echoBot);
            _echoBot.BotLogger = _botLogger;
            return _echoBot.Start();
        }
    }
}
