using Kahla.SDK.Factories;
using Kahla.SDK.Services;
using System;
using System.Threading.Tasks;

namespace Kahla.SDK.Abstract
{
    public class BotCommander<T> where T : BotBase
    {
        private readonly CommandFactory<T> _commandFactory;
        private readonly BotLogger _botLogger;

        public BotCommander(
            CommandFactory<T> commandFactory,
            BotLogger botLogger)
        {
            _commandFactory = commandFactory;
            _botLogger = botLogger;
        }

        public BotCommander<T> InjectHost(BotHost<T> botHost)
        {
            _commandFactory.InjectHost(botHost);
            return this;
        }

        public async Task Command()
        {
            var commanding = true;
            while (commanding)
            {
                _botLogger.WriteGrayNewLine($"K:\\Bots\\>");
                var command = Console.ReadLine();
                if (command.Length < 1)
                {
                    continue;
                }

                var handler = _commandFactory.ProduceHandler(command);
                if (handler == null)
                {
                    _botLogger.LogDanger($"Unknown command: {command}. Please try command: 'help' for help.");
                    continue;
                }
                commanding = await handler.Execute(command);
            }
        }
    }
}
