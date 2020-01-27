using Kahla.SDK.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kahla.SDK.Abstract
{
    public static class BotConfigurer
    {
        public static BotBase SelectBot(
            IEnumerable<BotBase> bots,
            SettingsService settingsService,
            BotLogger botLogger)
        {
            var builtBots = bots.ToList();
            if (!int.TryParse(settingsService["BotCoreIndex"]?.ToString(), out int code))
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
            }
            settingsService["BotCoreIndex"] = code;
            return builtBots[code];
        }
    }
}
