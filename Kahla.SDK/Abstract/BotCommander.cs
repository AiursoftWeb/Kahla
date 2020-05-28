using Aiursoft.Scanner.Interfaces;
using Aiursoft.Scanner.Services;
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
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly BotLogger _botLogger;

        public BotCommander(
            IServiceScopeFactory serviceProvider,
            BotLogger botLogger
            )
        {
            _serviceScopeFactory = serviceProvider;
            _botLogger = botLogger;
        }

        private ICommandHandler GetHandler(string command)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var handlers = scope.ServiceProvider.GetRequiredService<IEnumerable<ICommandHandler>>();
            foreach (var handler in handlers)
            {
                if (handler.CanHandle(command.ToLower().Trim()))
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
