using Aiursoft.Kahla.SDK.Abstract;
using Aiursoft.Kahla.SDK.Services;

namespace Aiursoft.Kahla.SDK.CommandHandlers
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

        public bool CanHandle(string command)
        {
            return command.StartsWith("logout");
        }

        public async Task<bool> Execute(string command)
        {
            await _botHost.ReleaseMonitorJob();
            await _botHost.LogOff();
            _botLogger.LogWarning("Successfully log out. Use command:`reboot` to reconnect.");
            return true;
        }
    }
}
