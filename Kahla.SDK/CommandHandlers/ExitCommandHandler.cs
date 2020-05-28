using Kahla.SDK.Abstract;
using System;
using System.Threading.Tasks;

namespace Kahla.SDK.CommandHandlers
{
    public class ExitCommandHandler<T> : ICommandHandler<T> where T : BotBase
    {
        public void InjectHost(BotHost<T> instance) { }
        public bool CanHandle(string command)
        {
            return command.StartsWith("exit");
        }
        public Task<bool> Execute(string command)
        {
            Environment.Exit(0);
            return Task.FromResult(true);
        }
    }
}
