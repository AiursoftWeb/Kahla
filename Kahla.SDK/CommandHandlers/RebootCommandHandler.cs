using Kahla.SDK.Abstract;
using System;
using System.Threading.Tasks;

namespace Kahla.SDK.CommandHandlers
{
    [CommandHandler("reboot")]
    public class RebootCommandHandler<T> : CommandHandlerBase<T> where T: BotBase
    {
        public RebootCommandHandler(BotCommander<T> botCommander) : base(botCommander)
        {
        }

        public async override Task Execute(string command)
        {
            await Task.Delay(0);
            Console.Clear();
            var _ = _botCommander._botHost.Connect().ConfigureAwait(false);
        }
    }
}
