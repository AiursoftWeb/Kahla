using Aiursoft.Scanner.Interfaces;
using Aiursoft.Scanner.Services;
using Kahla.SDK.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Kahla.SDK.Abstract
{
    public class BotCommander<T> : IScopedDependency where T : BotBase
    {
        private readonly IEnumerable<ICommandHandler> _handlers;
        private readonly BotLogger _botLogger;

        public BotCommander(
            BotLogger botLogger,
            IEnumerable<ICommandHandler> handlers)
        {
            _botLogger = botLogger;
            _handlers = handlers;
        }

        private ICommandHandler GetHandler(string command)
        {
            foreach(var handler in _handlers)
            {
                if(handler.CanHandle(command.ToLower().Trim()))
                {
                    return handler;
                }
            }
            return null;
        }

        public void RenderHeader()
        {
            _botLogger.WriteGrayNewLine($"K:\\Bots\\>");
        }

        public async Task Command()
        {
            await Task.Delay(1000);
            Console.Clear();
            while (true)
            {
                RenderHeader();
                var command = Console.ReadLine();
                if (command.Length < 1)
                {
                    continue;
                }

                var handler = GetHandler(command);
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
