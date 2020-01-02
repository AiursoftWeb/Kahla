using Aiursoft.XelNaga.Tools;
using Kahla.Bot.Services;
using Kahla.SDK.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Kahla.SDK.Abstract
{
    public static class BotConfigurer
    {
        public static List<Type> AllAccessiableClass()
        {
            var entry = Assembly.GetEntryAssembly();
            return entry
                .GetReferencedAssemblies()
                .ToList()
                .Select(t => Assembly.Load(t))
                .AddWith(entry)
                .SelectMany(t => t.GetTypes())
                .Where(t => !t.IsAbstract)
                .Where(t => !t.IsNestedPrivate)
                .Where(t => !t.IsGenericType)
                .Where(t => !t.IsInterface)
                .Where(t => !(t.Namespace?.StartsWith("System") ?? true))
                .ToList();
        }

        public static IServiceCollection AddBots(this IServiceCollection services)
        {
            var executingTypes = AllAccessiableClass()
                .Where(t => t.IsSubclassOf(typeof(BotBase)));
            foreach (var item in executingTypes)
            {
                services.AddScoped(typeof(BotBase), item);
            }
            return services;
        }

        public static BotBase SelectBot(
            IEnumerable<BotBase> bots,
            SettingsService settingsService,
            BotLogger botLogger)
        {
            var builtBots = bots.ToList();
            int code = Convert.ToInt32(settingsService.Read("BotCoreIndex"));
            if (!(code > 0))
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
                settingsService.Save("BotCoreIndex", code);
            }
            return builtBots[code];
        }
    }
}
