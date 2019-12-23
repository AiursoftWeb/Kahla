using Aiursoft.Pylon;
using Aiursoft.Pylon.Interfaces;
using Kahla.Bot.Services;
using Kahla.SDK.Abstract;
using Kahla.SDK.Models;
using Kahla.SDK.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.Bot
{
    public class StartUp : IScopedDependency
    {
        private readonly BotBase _echoBot;

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
            var builtBots = bots.ToList();
            int code = settingsService.Read().BotCoreIndex;
            if (code < 0)
            {
                botLogger.LogWarning("Select your bot:\n");
                for (int i = 0; i < builtBots.Count; i++)
                {
                    botLogger.LogInfo($"\t{i.ToString()} {builtBots[i].GetType().Name}");
                }
                while (true)
                {
                    botLogger.LogInfo($"Select bot:");
                    var codeString = Console.ReadLine().Trim();
                    if (!int.TryParse(codeString, out code) || code >= builtBots.Count)
                    {
                        botLogger.LogDanger($"Invalid item!");
                        continue;
                    }
                    break;
                }
                settingsService.Save(code);
            }
            var echoBot = builtBots[code];
            echoBot.BotLogger = botLogger;
            echoBot.AES = aes;
            echoBot.ConversationService = conversationService;
            echoBot.FriendshipService = friendshipService;
            echoBot.HomeService = homeService;
            echoBot.KahlaLocation = kahlaLocation;
            echoBot.AuthService = authService;
            echoBot.VersionService = versionService;
            echoBot.SettingsService = settingsService;
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
                .AddBots()
                .BuildServiceProvider()
                .GetService<IServiceScopeFactory>()
                .CreateScope();
        }

        public Task Start() => _echoBot.Start();
    }
}
