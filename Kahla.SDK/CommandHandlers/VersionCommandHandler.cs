using Kahla.SDK.Abstract;
using System.Threading.Tasks;

namespace Kahla.SDK.CommandHandlers
{
    [CommandHandler("version")]
    public class VersionCommandHandler<T> : CommandHandlerBase<T> where T : BotBase
    {
        public VersionCommandHandler(BotCommander<T> botCommander) : base(botCommander)
        {

        }

        public async override Task Execute(string command)
        {
            await _botCommander.BotHost.TestKahlaLive(_botCommander._kahlaLocation.ToString());
        }
    }
}
