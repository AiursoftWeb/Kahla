using Kahla.SDK.Abstract;
using System;
using System.Threading.Tasks;

namespace Kahla.SDK.CommandHandlers
{
    public class RebootCommandHandler<T> : ICommandHandler<T> where T : BotBase
    {
        private BotHost<T> _botHost;

        public void InjectHost(BotHost<T> instance)
        {
            _botHost = instance;
        }

        public bool CanHandle(string command)
        {
            return command.StartsWith("reboot");
        }

        public async Task<bool> Execute(string command)
        {
            await Task.Delay(0);
            Console.Clear();
            _botHost.ConnectTask = _botHost.Connect((websocketAddress) =>
            {
                _botHost.MonitorTask = _botHost.MonitorEvents(websocketAddress);
            });
            await _botHost.ConnectTask;
            return true;
        }
    }
}
