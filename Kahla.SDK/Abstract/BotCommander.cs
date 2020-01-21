using Aiursoft.Scanner.Interfaces;
using Kahla.SDK.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.SDK.Abstract
{
    public class BotCommander : IScopedDependency
    {
        private BotBase _botBase;
        private readonly ConversationService _conversationService;
        private readonly BotLogger _botLogger;

        public BotCommander(
            ConversationService conversationService,
            BotLogger botLogger)
        {
            _conversationService = conversationService;
            _botLogger = botLogger;
        }

        public BotCommander Init(BotBase botBase)
        {
            _botBase = botBase;
            return this;
        }

        public async Task Command()
        {
            while (true)
            {
                while (_botBase.ConnectingLock.CurrentCount == 0)
                {
                    await Task.Delay(1000);
                }
                var command = _botBase.BotLogger.ReadLine($"Bot:\\System\\{_botBase.Profile?.NickName}>");
                if (command.Length < 1)
                {
                    continue;
                }
                switch (command.ToLower().Trim())
                {
                    case "say":
                        var conversations = await _conversationService.AllAsync();
                        _botLogger.LogInfo($"");
                        foreach (var conversation in conversations.Items)
                        {
                            _botLogger.LogInfo($"ID: {conversation.ConversationId}\tName:\t{conversation.DisplayName}");
                        }
                        _botLogger.LogInfo($"");
                        var convId = _botLogger.ReadLine($"Enter conversation ID you want to say:");
                        var target = conversations.Items.FirstOrDefault(t => t.ConversationId.ToString() == convId);
                        if (target == null)
                        {
                            _botLogger.LogDanger($"Can't find conversation with ID: {convId}");
                            break;
                        }
                        var toSay = _botLogger.ReadLine($"Enter the message you want to send to '{target.DisplayName}':");
                        await _botBase.SendMessage(toSay, target.ConversationId, target.AesKey);
                        _botLogger.LogSuccess($"Sent.");
                        break;
                    case "exit":
                        await _botBase.LogOff();
                        return;
                    case "version":
                        await _botBase.TestKahlaLive();
                        break;
                    case "conv":
                        conversations = await _conversationService.AllAsync();
                        _botLogger.LogSuccess($"Successfully get all your conversations.");
                        foreach (var conversation in conversations.Items)
                        {
                            _botLogger.LogInfo($"ID:\t{conversation.ConversationId}");
                            _botLogger.LogInfo($"Name:\t{conversation.DisplayName}");
                            _botLogger.LogInfo($"Online:\t{conversation.Online}");
                            _botLogger.LogInfo($"Type:\t{conversation.Discriminator}");
                            _botLogger.LogInfo($"Last:\t{conversation.LatestMessage}");
                            _botLogger.LogInfo($"Time:\t{conversation.LatestMessageTime}");
                            if (conversation.UnReadAmount > 0)
                            {
                                _botLogger.LogDanger($"Unread:\t**{conversation.UnReadAmount}**\n");
                            }
                            else
                            {
                                _botLogger.LogInfo($"Unread:\t{conversation.UnReadAmount}\n");
                            }
                        }
                        break;
                    case "clear":
                        Console.Clear();
                        break;
                    case "logout":
                        await _botBase.LogOff();
                        _botLogger.LogWarning($"Successfully log out. Use command:`rec` to reconnect.");
                        break;
                    case "rec":
                        var _ = _botBase.Connect().ConfigureAwait(false);
                        break;
                    case "help":
                        _botLogger.LogInfo($"Kahla bot commands:");

                        _botLogger.LogInfo($"\r\nConversation");
                        _botLogger.LogInfo($"\tconv\tShow all conversations.");
                        _botLogger.LogInfo($"\tsay\tSay something to someone.");
                        _botLogger.LogInfo($"\tb\tBroadcast to all conversations.");
                        _botLogger.LogInfo($"\tclear\tClear console.");

                        _botLogger.LogInfo($"\r\nGroup");
                        _botLogger.LogInfo($"\tm\tMute all groups.");
                        _botLogger.LogInfo($"\tu\tUnmute all groups.");

                        _botLogger.LogInfo($"\r\nNetwork");
                        _botLogger.LogInfo($"\trec\tReconnect to Stargate.");
                        _botLogger.LogInfo($"\tlogout\tLogout.");

                        _botLogger.LogInfo($"\r\nProgram");
                        _botLogger.LogInfo($"\thelp\tShow help.");
                        _botLogger.LogInfo($"\tversion\tCheck and show version info.");
                        _botLogger.LogInfo($"\texit\tQuit bot.");
                        _botLogger.LogInfo($"");
                        break;
                    default:
                        _botLogger.LogDanger($"Unknown command: {command}. Please try command: 'help' for help.");
                        break;
                }
            }
        }
    }
}
