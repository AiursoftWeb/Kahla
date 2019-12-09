using Aiursoft.Pylon;
using Aiursoft.Pylon.Interfaces;
using Kahla.Bot.Bots;
using Kahla.Bot.Services;
using Kahla.SDK.Core;
using Kahla.SDK.Models;
using Kahla.SDK.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Threading.Tasks;

namespace Kahla.Bot
{
    public class StartUp : IScopedDependency
    {
        private readonly ConversationService _conversationService;
        private readonly BotListener _botListener;
        private readonly BotCommander _botCommander;
        private readonly BotLogger _botLogger;
        private readonly EchoBot _echoBot;
        private readonly AES _aes;

        public StartUp(
            ConversationService conversationService,
            BotListener botListener,
            BotCommander botCommander,
            BotLogger botLogger,
            EchoBot echoBot,
            AES aes)
        {
            _conversationService = conversationService;
            _botListener = botListener;
            _botCommander = botCommander;
            _botLogger = botLogger;
            _echoBot = echoBot;
            _aes = aes;
        }

        public static IServiceScope ConfigureServices()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };

            return new ServiceCollection()
                .AddAiurDependencies<KahlaUser>("Kahla")
                .BuildServiceProvider()
                .GetService<IServiceScopeFactory>()
                .CreateScope();
        }

        public Task Start()
        {
            _echoBot.BotCommander = _botCommander.WithBot(_echoBot);
            _echoBot.BotListener = _botListener.WithBot(_echoBot);
            _echoBot.BotLogger = _botLogger;
            _echoBot.AES = _aes;
            _echoBot.ConversationService = _conversationService;
            return _echoBot.Start();
        }
    }
}
