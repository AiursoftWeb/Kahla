using Aiursoft.Pylon.Interfaces;
using Kahla.EchoBot.Services;
using System;

namespace Kahla.EchoBot.Core
{
    public class BotCommander : ISingletonDependency
    {
        private readonly BotLogger _botLogger;

        public BotCommander(BotLogger botLogger)
        {
            _botLogger = botLogger;
        }

        public void Command()
        {
            while (true)
            {
                var command = Console.ReadLine();
                switch (command.ToLower().Trim())
                {
                    case "exit":
                        return;
                    default:
                        _botLogger.LogDanger($"Unknown command: {command}. Please try 'help'.");
                        break;
                }
            }
        }
    }
}
