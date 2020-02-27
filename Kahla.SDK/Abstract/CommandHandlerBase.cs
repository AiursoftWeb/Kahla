using System.Threading.Tasks;

namespace Kahla.SDK.Abstract
{
    public abstract class CommandHandlerBase
    {
        protected readonly BotCommander _botCommander;

        public CommandHandlerBase(BotCommander botCommander)
        {
            _botCommander = botCommander;
        }

        public abstract Task Execute(string command);
    }
}
