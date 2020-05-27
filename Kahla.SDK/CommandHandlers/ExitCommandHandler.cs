using Kahla.SDK.Abstract;
using System;
using System.Threading.Tasks;

namespace Kahla.SDK.CommandHandlers
{
    [CommandHandler("exit")]
    public class ExitCommandHandler : CommandHandlerBase
    {
        public ExitCommandHandler(IBotCommander botCommander) : base(botCommander)
        {
        }

        public async override Task Execute(string command)
        {
            await _botCommander.BotHost.LogOff();
            Environment.Exit(0);
        }
    }
}
