using Kahla.SDK.Abstract;
using System;
using System.Threading.Tasks;

namespace Kahla.SDK.CommandHandlers
{
    public class ExitCommandHandler : ICommandHandler
    {
        public  bool CanHandle(string command)
        {
            return command.StartsWith("exit");
        }
        public  Task Execute(string command)
        {
            Environment.Exit(0);
            return Task.CompletedTask;
        }
    }
}
