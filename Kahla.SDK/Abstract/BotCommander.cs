using Aiursoft.Scanner.Interfaces;
using Aiursoft.Scanner.Services;
using Kahla.SDK.Services;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Kahla.SDK.Abstract
{
    public class BotCommander<T> : ITransientDependency, IBotCommander where T : BotBase
    {
        public IBotHost BotHost { get; set; }
        public  ConversationService ConversationService { get; set; }
        public  BotLogger BotLogger { get; set; }
        public  KahlaLocation _kahlaLocation { get; set; }
        public  AES _aes { get; set; }

        public BotCommander(
            ConversationService conversationService,
            BotLogger botLogger,
            KahlaLocation kahlaLocation,
            AES aes)
        {
            ConversationService = conversationService;
            BotLogger = botLogger;
            _kahlaLocation = kahlaLocation;
            _aes = aes;
        }

        public BotCommander<T> Init(BotHost<T> botBase)
        {
            BotHost = botBase;
            return this;
        }

        public async Task BlockIfConnecting()
        {
            while (BotHost.ConnectingLock.CurrentCount == 0)
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

        public void RenderHeader()
        {
            BotLogger.WriteGrayNewLine($"K:\\Bots\\{BotHost.GetType().Name}\\{BotHost.BuildBot.Profile?.NickName}>");
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

                var handler = GetHandler(command);
                if (handler == null)
                {
                    BotLogger.LogDanger($"Unknown command: {command}. Please try command: 'help' for help.");
                    continue;
                }
                var handlerObject = Activator.CreateInstance(handler) as CommandHandlerBase;
                await handlerObject.Execute(command);
            }
        }
    }
}
