using Kahla.SDK.Abstract;
using Kahla.SDK.Services;
using System.Threading.Tasks;

namespace Kahla.SDK.CommandHandlers
{
    public class LogoutCommandHandler<T> : ICommandHandler where T : BotBase
    {
        private readonly BotLogger _botLogger;
        private readonly BotHost<T> _botHost;

        public LogoutCommandHandler(
            BotLogger botLogger,
            BotHost<T> botHost)
        {
            _botLogger = botLogger;
            _botHost = botHost;
        }

        public  bool CanHandle(string command)
        {
            return command.StartsWith("logout");
        }

        public async  Task Execute(string command)
        {
            await _botHost.LogOff();
            _botLogger.LogWarning($"Successfully log out. Use command:`reboot` to reconnect.");
        }
    }
}
