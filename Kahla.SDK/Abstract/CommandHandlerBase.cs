using System.Threading.Tasks;

namespace Kahla.SDK.Abstract
{
    public abstract class CommandHandlerBase
    {
        protected readonly IBotCommander _botCommander;

        public CommandHandlerBase(IBotCommander botCommander)
        {
            _botCommander = botCommander;
        }

        public abstract Task Execute(string command);
    }
}
