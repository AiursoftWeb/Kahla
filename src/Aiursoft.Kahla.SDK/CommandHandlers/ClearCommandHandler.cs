using Aiursoft.Kahla.SDK.Abstract;

namespace Aiursoft.Kahla.SDK.CommandHandlers
{
    public class ClearCommandHandler<T> : ICommandHandler<T> where T : BotBase
    {
        public void InjectHost(BotHost<T> instance) { }
        public bool CanHandle(string command)
        {
            return command.StartsWith("clear");
        }

        public Task<bool> Execute(string command)
        {
            Console.Clear();
            return Task.FromResult(true);
        }
    }
}
