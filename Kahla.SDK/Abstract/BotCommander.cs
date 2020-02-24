using Aiursoft.Scanner.Interfaces;
using Aiursoft.Scanner.Services;
using Kahla.SDK.Services;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Kahla.SDK.Abstract
{
    public class BotCommander : ITransientDependency
    {
        public BotBase _botBase;
        public readonly ConversationService _conversationService;
        public readonly BotLogger _botLogger;
        public readonly KahlaLocation _kahlaLocation;
        public readonly AES _aes;

        public BotCommander(
            ConversationService conversationService,
            BotLogger botLogger,
            KahlaLocation kahlaLocation,
            AES aes)
        {
            _conversationService = conversationService;
            _botLogger = botLogger;
            _kahlaLocation = kahlaLocation;
            _aes = aes;
        }

        public BotCommander Init(BotBase botBase)
        {
            _botBase = botBase;
            return this;
        }

        public async Task BlockIfConnecting()
        {
            while (_botBase.ConnectingLock.CurrentCount == 0)
            {
                await Task.Delay(1000);
            }
        }

        /// <summary>
        /// Get a handler which can handle the command.
        /// </summary>
        /// <param name="command"></param>
        /// <returns>The handler or null if not found.</returns>
        private Type GetHandler(string command)
        {
            var scannedHandler = new ClassScanner().AllAccessiableClass(false, false)
                    .Where(t => t.CustomAttributes.Any(a => a.AttributeType == typeof(CommandHandlerAttribute)))
                    .Where(t => t.IsSubclassOf(typeof(CommandHandlerBase)))
                    .Where(t =>
                        t.GetCustomAttributes(typeof(CommandHandlerAttribute), false)
                        .Any(a => command.ToLower().StartsWith(((CommandHandlerAttribute)a).Command.ToLower())));
            return scannedHandler.FirstOrDefault();
        }

        public async Task Command()
        {
            await BlockIfConnecting();
            await Task.Delay(1000);
            Console.Clear();
            while (true)
            {
                await BlockIfConnecting();
                var command = _botBase.BotLogger.ReadLine($"K:\\Bots\\{_botBase.GetType().Name}\\{_botBase.Profile?.NickName}>");
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
                var handlerObject = Assembly.GetAssembly(handler)
                    .CreateInstance(handler.FullName, true, BindingFlags.Default, null, new object[1] { this }, null, null)
                    as CommandHandlerBase;
                await handlerObject.Execute(command);
            }
        }
    }
}
