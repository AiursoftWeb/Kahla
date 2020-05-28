using Kahla.SDK.Abstract;
using System;
using System.Threading.Tasks;

namespace Kahla.SDK.CommandHandlers
{
    public class ClearCommandHandler<T> : ICommandHandler<T> where T : BotBase
    {
        public void InjectHost(BotHost<T> instance) { }
        public bool CanHandle(string command)
        {
            return command.StartsWith("clear");
        }

        public async Task<bool> Execute(string command)
        {
            Console.Clear();
            return true;
        }
    }
}
