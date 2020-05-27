using Kahla.SDK.Abstract;
using System.Threading.Tasks;

namespace Kahla.SDK.CommandHandlers
{
    [CommandHandler("help")]
    public class HelpCommandHandler<T> : CommandHandlerBase<T> where T : BotBase
    {
        public HelpCommandHandler(BotCommander<T> botCommander) : base(botCommander)
        {
        }

        public async override Task Execute(string command)
        {
            await Task.Delay(0);
            _botCommander._botLogger.LogInfo($"Kahla bot commands:");

            _botCommander._botLogger.LogInfo($"\r\nConversation");
            _botCommander._botLogger.LogInfo($"\tconv\t\tShow all conversations.");
            _botCommander._botLogger.LogInfo($"\tconv [ID]\tShow messages in one conversation.");
            _botCommander._botLogger.LogInfo($"\treq\t\tShow all requests.");
            _botCommander._botLogger.LogInfo($"\tsay\t\tSay something to someone.");
            _botCommander._botLogger.LogInfo($"\tb\t\tBroadcast to all conversations.");
            _botCommander._botLogger.LogInfo($"\tclear\t\tClear console.");

            _botCommander._botLogger.LogInfo($"\r\nGroup");
            _botCommander._botLogger.LogInfo($"\tm\t\tMute all groups.");
            _botCommander._botLogger.LogInfo($"\tu\t\tUnmute all groups.");

            _botCommander._botLogger.LogInfo($"\r\nNetwork");
            _botCommander._botLogger.LogInfo($"\treboot\t\tReconnect to Stargate.");
            _botCommander._botLogger.LogInfo($"\tlogout\t\tLogout.");

            _botCommander._botLogger.LogInfo($"\r\nProgram");
            _botCommander._botLogger.LogInfo($"\thelp\t\tShow help.");
            _botCommander._botLogger.LogInfo($"\tversion\t\tCheck and show version info.");
            _botCommander._botLogger.LogInfo($"\texit\t\tQuit bot.");
            _botCommander._botLogger.LogInfo($"");
        }
    }
}
