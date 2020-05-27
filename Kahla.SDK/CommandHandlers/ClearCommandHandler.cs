using Kahla.SDK.Abstract;
using System;
using System.Threading.Tasks;

namespace Kahla.SDK.CommandHandlers
{
    [CommandHandler("clear")]
    public class ClearCommandHandler<T> : CommandHandlerBase<T> where T : BotBase
    {
        public ClearCommandHandler(BotCommander<T> botCommander) : base(botCommander)
        {
        }

        public async override Task Execute(string command)
        {
            await Task.Delay(0);
            Console.Clear();
        }
    }
}
