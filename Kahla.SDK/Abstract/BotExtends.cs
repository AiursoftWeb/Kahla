using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Kahla.SDK.Abstract
{
    public static class BotExtends
    {
        private static IEnumerable<Type> ScanBots()
        {
            var bots = Assembly
                .GetEntryAssembly()
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(BotBase)));
            return bots;
        }

        public static IServiceProvider AddBots(this IServiceCollection services)
        {
            // Register the bots.
            foreach (var botType in ScanBots())
            {
                services.AddSingleton(botType);
                services.AddSingleton(typeof(BotBase), botType);
            }

            // Get a service provider to get bots and factory.
            var serviceProvider = services.BuildServiceProvider();
            foreach (var bot in serviceProvider.GetServices<BotBase>())
            {
                serviceProvider.GetService<BotFactory>().BuildBotProperties(bot);
            }
            foreach (var botType in ScanBots())
            {
                var botService = serviceProvider.GetService(botType) as BotBase;
                serviceProvider.GetService<BotFactory>().BuildBotProperties(botService);
            }
            return serviceProvider;
        }
    }
}
