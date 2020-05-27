using Kahla.SDK.Abstract;
using System;
using System.Threading.Tasks;

namespace Kahla.SDK.CommandHandlers
{
    [CommandHandler("exit")]
    public class ExitCommandHandler<T> : CommandHandlerBase<T> where T : BotBase
    {
        public ExitCommandHandler(BotCommander<T> botCommander) : base(botCommander)
        {
        }

        public async override Task Execute(string command)
        {
            await _botCommander._botHost.LogOff();
            Environment.Exit(0);
        }
    }
}
