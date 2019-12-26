using Aiursoft.Pylon;
using Aiursoft.Pylon.Interfaces;
using Kahla.Bot.Services;
using Kahla.SDK.Abstract;
using Kahla.SDK.Models;
using Kahla.SDK.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kahla.Bot
{
    public class StartUp : IScopedDependency
    {
        private readonly BotBase _bot;

        public StartUp(
            ConversationService conversationService,
            FriendshipService friendshipService,
            AuthService authService,
            HomeService homeService,
            KahlaLocation kahlaLocation,
            BotLogger botLogger,
            IEnumerable<BotBase> bots,
            VersionService versionService,
            SettingsService settingsService,
            AES aes)
        {
            var bot = BotConfigurer.SelectBot(bots, settingsService, botLogger);
            bot.BotLogger = botLogger;
            bot.AES = aes;
            bot.ConversationService = conversationService;
            bot.FriendshipService = friendshipService;
            bot.HomeService = homeService;
            bot.KahlaLocation = kahlaLocation;
            bot.AuthService = authService;
            bot.VersionService = versionService;
            bot.SettingsService = settingsService;
            _bot = bot;
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
                .AddBots()
                .BuildServiceProvider()
                .GetService<IServiceScopeFactory>()
                .CreateScope();
        }

        public Task Start() => _bot.Start();
    }
}
