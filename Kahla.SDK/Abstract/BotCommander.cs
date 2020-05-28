using Aiursoft.Scanner.Interfaces;
using Aiursoft.Scanner.Services;
using Kahla.SDK.Factories;
using Kahla.SDK.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Kahla.SDK.Abstract
{
    public class BotCommander<T> : IScopedDependency where T : BotBase
    {
        private readonly CommandFactory<T> _commandFactory;
        private readonly BotLogger _botLogger;
        private BotHost<T> _instance;

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
            await Task.Delay(1000);
            Console.Clear();
            while (true)
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
                await handler.Execute(command);
            }
        }


    }
}
