using Kahla.SDK.Abstract;
using Kahla.SDK.Services;
using System.Threading.Tasks;

namespace Kahla.SDK.CommandHandlers
{
    public class LogoutCommandHandler<T> : ICommandHandler<T> where T : BotBase
    {
        private readonly BotLogger _botLogger;
        private BotHost<T> _botHost;

        public LogoutCommandHandler(
            BotLogger botLogger)
        {
            _botLogger = botLogger;
        }

        public void InjectHost(BotHost<T> instance)
        {
            _botHost = instance;
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
