using Aiursoft.Pylon;
using Aiursoft.Pylon.Interfaces;
using Kahla.Bot.Bots;
using Kahla.Bot.Services;
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
        private readonly EchoBot _echoBot;

        public StartUp(
            ConversationService conversationService,
            FriendshipService friendshipService,
            AuthService authService,
            HomeService homeService,
            KahlaLocation kahlaLocation,
            BotLogger botLogger,
            EchoBot echoBot,
            VersionService versionService,
            AES aes)
        {
            echoBot.BotLogger = botLogger;
            echoBot.AES = aes;
            echoBot.ConversationService = conversationService;
            echoBot.FriendshipService = friendshipService;
            echoBot.HomeService = homeService;
            echoBot.KahlaLocation = kahlaLocation;
            echoBot.AuthService = authService;
            echoBot.VersionService = versionService;
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

            return new ServiceCollection()
                .AddAiurDependencies<KahlaUser>("Kahla")
                .BuildServiceProvider()
                .GetService<IServiceScopeFactory>()
                .CreateScope();
        }

        public Task Start() => _echoBot.Start();
    }
}
