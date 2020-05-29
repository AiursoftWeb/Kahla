using Aiursoft.Scanner.Interfaces;
using Kahla.SDK.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kahla.SDK.Abstract
{
    public class BotSelector : ISingletonDependency
    {
        private readonly IEnumerable<BotBase> _bots;
        private readonly SettingsService _settingsService;
        private readonly BotLogger _botLogger;

        public BotSelector(
            IEnumerable<BotBase> bots,
            SettingsService settingsService,
            BotLogger botLogger)
        {
            _bots = bots;
            _settingsService = settingsService;
            _botLogger = botLogger;
        }

        public BotBase SelectBot()
        {
            var builtBots = _bots.ToList();
            if (!int.TryParse(_settingsService["BotCoreIndex"]?.ToString(), out int code))
            {
                _botLogger.LogWarning("Select your bot:\n");
                for (int i = 0; i < builtBots.Count; i++)
                {
                    _botLogger.LogInfo($"\t{i} {builtBots[i].GetType().Name}");
                }
                while (true)
                {
                    _botLogger.LogInfo($"Select bot:");
                    var codeString = Console.ReadLine().Trim();
                    if (!int.TryParse(codeString, out code) || code >= builtBots.Count)
                    {
                        _botLogger.LogDanger($"Invalid item!");
                        continue;
                    }
                    break;
                }
            }
            _settingsService["BotCoreIndex"] = code;
            return builtBots[code];
        }
    }
}
