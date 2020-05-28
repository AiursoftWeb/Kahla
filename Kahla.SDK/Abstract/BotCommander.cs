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
        private BotHost<T> _instance;

        public BotCommander(
            IServiceScopeFactory serviceProvider,
            BotLogger botLogger)
        {
            _serviceScopeFactory = serviceProvider;
            _botLogger = botLogger;
        }

        public BotCommander<T> InjectHost(BotHost<T> instance)
        {
            _instance = instance;
            return this;
        }

        private ICommandHandler<T> GetHandler(string command)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var handlers = scope.ServiceProvider.GetRequiredService<IEnumerable<ICommandHandler<T>>>();
            foreach (var handler in handlers)
            {
                handler.InjectHost(_instance);
                if (handler.CanHandle(command.ToLower().Trim()))
                {
                    return handler;
                }
            }
            return null;
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
