using Kahla.SDK.Abstract;

namespace Kahla.SDK.CommandHandlers
{
    public class ExitCommandHandler<T> : ICommandHandler<T> where T : BotBase
    {
        private BotHost<T> _botHost;
        public void InjectHost(BotHost<T> instance)
        {
            _botHost = instance;
        }
        public bool CanHandle(string command)
        {
            return command.StartsWith("exit");
        }
        public async Task<bool> Execute(string command)
        {
            await _botHost.ReleaseMonitorJob();
            return false;
        }
    }
}
