using Aiursoft.Scanner.Interfaces;
using Aiursoft.Scanner.Services;
using Kahla.SDK.Services;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Kahla.SDK.Abstract
{
    public class BotCommander<T> : ITransientDependency where T : BotBase
    {
        public BotHost<T> _botHost;
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

        public BotCommander<T> Init(BotHost<T> botBase)
        {
            _botHost = botBase;
            return this;
        }

        public async Task BlockIfConnecting()
        {
            while (_botHost.ConnectingLock.CurrentCount == 0)
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
                    .Where(t => t.IsSubclassOf(typeof(CommandHandlerBase<T>)))
                    .Where(t =>
                        t.GetCustomAttributes(typeof(CommandHandlerAttribute), false)
                        .Any(a => command.ToLower().StartsWith(((CommandHandlerAttribute)a).Command.ToLower())));
            return scannedHandler.FirstOrDefault();
        }

        public void RenderHeader()
        {
            _botLogger.WriteGrayNewLine($"K:\\Bots\\{_botHost.GetType().Name}\\{_botHost._bot.Profile?.NickName}>");
        }

        public async Task Command()
        {
            await BlockIfConnecting();
            await Task.Delay(1000);
            Console.Clear();
            while (true)
            {
                await BlockIfConnecting();
                RenderHeader();
                var command = Console.ReadLine();
                if (command.Length < 1)
                {
                    continue;
                }

                var handler = GetHandler(command).MakeGenericType(typeof(T));
                if (handler == null)
                {
                    _botLogger.LogDanger($"Unknown command: {command}. Please try command: 'help' for help.");
                    continue;
                }
                var handlerObject = Activator.CreateInstance(handler) as CommandHandlerBase<T>;
                await handlerObject.Execute(command);
            }
        }
    }
}
