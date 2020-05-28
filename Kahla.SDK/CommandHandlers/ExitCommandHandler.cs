﻿using Kahla.SDK.Abstract;
using System.Threading.Tasks;

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
        public Task<bool> Execute(string command)
        {
            _botHost.ReleaseMonitorJob();
            return Task.FromResult(false);
        }
    }
}
