using System.Threading.Tasks;

namespace Kahla.SDK.Abstract
{
    public abstract class CommandHandlerBase<T> where T : BotBase
    {
        protected readonly BotCommander<T> _botCommander;

        public CommandHandlerBase(BotCommander<T> botCommander)
        {
            _botCommander = botCommander;
        }

        public abstract Task Execute(string command);
    }
}
