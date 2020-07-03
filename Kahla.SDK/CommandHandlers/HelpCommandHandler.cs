using Kahla.SDK.Abstract;
using Kahla.SDK.Services;
using System.Threading.Tasks;

namespace Kahla.SDK.CommandHandlers
{
    public class HelpCommandHandler<T> : ICommandHandler<T> where T : BotBase
    {
        private readonly BotLogger _botLogger;

        public HelpCommandHandler(BotLogger botLogger)
        {
            _botLogger = botLogger;
        }
        public void InjectHost(BotHost<T> instance) { }
        public bool CanHandle(string command)
        {
            return command.StartsWith("help");
        }

        public Task<bool> Execute(string command)
        {
            _botLogger.LogInfo("Kahla bot commands:");

            _botLogger.LogInfo("\r\nConversation");
            _botLogger.LogInfo("\tconv\t\tShow all conversations.");
            _botLogger.LogInfo("\tconv [ID]\tShow messages in one conversation.");
            _botLogger.LogInfo("\treq\t\tShow all requests.");
            _botLogger.LogInfo("\tsay\t\tSay something to someone.");
            _botLogger.LogInfo("\tb\t\tBroadcast to all conversations.");
            _botLogger.LogInfo("\tclear\t\tClear console.");

            _botLogger.LogInfo("\r\nGroup");
            _botLogger.LogInfo("\tm\t\tMute all groups.");
            _botLogger.LogInfo("\tu\t\tUnmute all groups.");

            _botLogger.LogInfo("\r\nNetwork");
            _botLogger.LogInfo("\treboot\t\tReconnect to Stargate.");
            _botLogger.LogInfo("\tlogout\t\tLogout.");

            _botLogger.LogInfo("\r\nProgram");
            _botLogger.LogInfo("\thelp\t\tShow help.");
            _botLogger.LogInfo("\tversion\t\tCheck and show version info.");
            _botLogger.LogInfo("\texit\t\tQuit bot.");
            _botLogger.LogInfo("");
            return Task.FromResult(true);
        }
    }
}
