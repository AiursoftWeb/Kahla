using Aiursoft.Scanner.Interfaces;
using Aiursoft.Scanner.Services;
using Kahla.SDK.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.SDK.Abstract
{
    public class BotCommander : ITransientDependency
    {
        private BotBase _botBase;
        private readonly ConversationService _conversationService;
        private readonly BotLogger _botLogger;
        private readonly AES _aes;

        public BotCommander(
            ConversationService conversationService,
            BotLogger botLogger,
            AES aes)
        {
            _conversationService = conversationService;
            _botLogger = botLogger;
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
                }


                //switch (command.ToLower().Trim())
                //{
                //    case "say":
                //        var conversations = await _conversationService.AllAsync();
                //        _botLogger.LogInfo($"");
                //        foreach (var conversation in conversations.Items)
                //        {
                //            _botLogger.LogInfo($"ID: {conversation.ConversationId}\tName:\t{conversation.DisplayName}");
                //        }
                //        _botLogger.LogInfo($"");
                //        var convId = _botLogger.ReadLine($"Enter conversation ID you want to say:");
                //        var target = conversations.Items.FirstOrDefault(t => t.ConversationId.ToString() == convId);
                //        if (target == null)
                //        {
                //            _botLogger.LogDanger($"Can't find conversation with ID: {convId}");
                //            break;
                //        }
                //        var toSay = _botLogger.ReadLine($"Enter the message you want to send to '{target.DisplayName}':");
                //        await _botBase.SendMessage(toSay, target.ConversationId, target.AesKey);
                //        _botLogger.LogSuccess($"Sent.");
                //        break;
                //    case "exit":
                //        await _botBase.LogOff();
                //        return;
                //    case "version":
                //        await _botBase.TestKahlaLive();
                //        break;
                //    case "conv":
                //        conversations = await _conversationService.AllAsync();
                //        _botLogger.LogSuccess($"Successfully get all your conversations.");
                //        foreach (var conversation in conversations.Items)
                //        {
                //            var online = conversation.Online ? "online" : "offline";
                //            _botLogger.LogInfo($"Name:\t{conversation.DisplayName}");
                //            _botLogger.LogInfo($"ID:\t{conversation.ConversationId}\t{online}\t\t{conversation.Discriminator}");
                //            if (!string.IsNullOrWhiteSpace(conversation.LatestMessage))
                //            {
                //                _botLogger.LogInfo($"Last:\t{_aes.OpenSSLDecrypt(conversation.LatestMessage, conversation.AesKey)}");
                //                _botLogger.LogInfo($"Time:\t{conversation.LatestMessageTime}");
                //            }
                //            if (conversation.UnReadAmount > 0)
                //            {
                //                _botLogger.LogDanger($"Unread:\t**{conversation.UnReadAmount}**");
                //            }
                //            else
                //            {
                //                _botLogger.LogInfo($"Unread:\t{conversation.UnReadAmount}");
                //            }
                //            if (conversation.SomeoneAtMe)
                //            {
                //                _botLogger.LogWarning($"At!");
                //            }
                //            _botLogger.LogInfo($"\n");
                //        }
                //        break;
                //    case "clear":
                //        Console.Clear();
                //        break;
                //    case "logout":
                //        await _botBase.LogOff();
                //        _botLogger.LogWarning($"Successfully log out. Use command:`reboot` to reconnect.");
                //        break;
                //    case "reboot":
                //        Console.Clear();
                //        var _ = _botBase.Connect().ConfigureAwait(false);
                //        break;
                //    case "help":
                //        _botLogger.LogInfo($"Kahla bot commands:");

                //        _botLogger.LogInfo($"\r\nConversation");
                //        _botLogger.LogInfo($"\tconv\tShow all conversations.");
                //        _botLogger.LogInfo($"\tsay\tSay something to someone.");
                //        _botLogger.LogInfo($"\tb\tBroadcast to all conversations.");
                //        _botLogger.LogInfo($"\tclear\tClear console.");

                //        _botLogger.LogInfo($"\r\nGroup");
                //        _botLogger.LogInfo($"\tm\tMute all groups.");
                //        _botLogger.LogInfo($"\tu\tUnmute all groups.");

                //        _botLogger.LogInfo($"\r\nNetwork");
                //        _botLogger.LogInfo($"\treboot\tReconnect to Stargate.");
                //        _botLogger.LogInfo($"\tlogout\tLogout.");

                //        _botLogger.LogInfo($"\r\nProgram");
                //        _botLogger.LogInfo($"\thelp\tShow help.");
                //        _botLogger.LogInfo($"\tversion\tCheck and show version info.");
                //        _botLogger.LogInfo($"\texit\tQuit bot.");
                //        _botLogger.LogInfo($"");
                //        break;
                //    default:
                //        break;
                //}
            }
        }
    }
}
