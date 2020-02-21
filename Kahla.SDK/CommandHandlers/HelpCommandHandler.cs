using Kahla.SDK.Abstract;
using System.Threading.Tasks;

namespace Kahla.SDK.CommandHandlers
{
    [CommandHandler("help")]
    public class HelpCommandHandler : CommandHandlerBase
    {
        public HelpCommandHandler(BotCommander botCommander) : base(botCommander)
        {
        }

        public async override Task Execute(string command)
        {
            await Task.Delay(0);
            _botCommander._botLogger.LogInfo($"Kahla bot commands:");

            _botCommander._botLogger.LogInfo($"\r\nConversation");
            _botCommander._botLogger.LogInfo($"\tconv\tShow all conversations.");
            _botCommander._botLogger.LogInfo($"\tsay\tSay something to someone.");
            _botCommander._botLogger.LogInfo($"\tb\tBroadcast to all conversations.");
            _botCommander._botLogger.LogInfo($"\tclear\tClear console.");

            _botCommander._botLogger.LogInfo($"\r\nGroup");
            _botCommander._botLogger.LogInfo($"\tm\tMute all groups.");
            _botCommander._botLogger.LogInfo($"\tu\tUnmute all groups.");

            _botCommander._botLogger.LogInfo($"\r\nNetwork");
            _botCommander._botLogger.LogInfo($"\treboot\tReconnect to Stargate.");
            _botCommander._botLogger.LogInfo($"\tlogout\tLogout.");

            _botCommander._botLogger.LogInfo($"\r\nProgram");
            _botCommander._botLogger.LogInfo($"\thelp\tShow help.");
            _botCommander._botLogger.LogInfo($"\tversion\tCheck and show version info.");
            _botCommander._botLogger.LogInfo($"\texit\tQuit bot.");
            _botCommander._botLogger.LogInfo($"");
        }
    }
}
