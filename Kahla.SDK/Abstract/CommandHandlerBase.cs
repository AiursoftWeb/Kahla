namespace Kahla.SDK.Abstract
{
    public class CommandHandlerBase
    {
        protected readonly BotCommander _botCommander;

        public CommandHandlerBase(BotCommander botCommander)
        {
            _botCommander = botCommander;
        }
    }
}
