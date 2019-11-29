using Aiursoft.Pylon.Interfaces;
using Kahla.EchoBot.Services;
using System;
using System.Threading.Tasks;

namespace Kahla.EchoBot.Core
{
    public class BotCommander : ISingletonDependency
    {
        private readonly BotLogger _botLogger;

        public BotCommander(BotLogger botLogger)
        {
            _botLogger = botLogger;
        }

        public async Task Command()
        {
            await Task.Delay(0);
            while (true)
            {
                var command = Console.ReadLine();
                if (command.Length < 1)
                {
                    continue;
                }
                switch (command.ToLower().Trim()[0])
                {
                    case 'q':
                        Environment.Exit(0);
                        return;
                    case 'h':
                        _botLogger.LogInfo($"Kahla bot commands:");

                        _botLogger.LogInfo($"\r\nConversation");
                        _botLogger.LogInfo($"\ta\tShow all conversations.");
                        _botLogger.LogInfo($"\ts\tSay something to someone.");
                        _botLogger.LogInfo($"\tb\tBroadcast to all conversations.");

                        _botLogger.LogInfo($"\r\nGroup");
                        _botLogger.LogInfo($"\tm\tMute all groups.");
                        _botLogger.LogInfo($"\tu\tUnmute all groups.");

                        _botLogger.LogInfo($"\r\nNetwork");
                        _botLogger.LogInfo($"\tr\tReconnect to Stargate.");
                        _botLogger.LogInfo($"\tl\tLogout.");

                        _botLogger.LogInfo($"\r\nProgram");
                        _botLogger.LogInfo($"\th\tShow help.");
                        _botLogger.LogInfo($"\tq\tQuit bot.");
                        break;
                    default:
                        _botLogger.LogDanger($"Unknown command: {command}. Please try command: 'h' for help.");
                        break;
                }
            }
        }
    }
}
