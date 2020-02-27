using Kahla.SDK.Abstract;
using System.Threading.Tasks;

namespace Kahla.SDK.CommandHandlers
{
    [CommandHandler("version")]
    public class VersionCommandHandler : CommandHandlerBase
    {
        public VersionCommandHandler(
            BotCommander botCommander) : base(botCommander)
        {

        }

        public async override Task Execute(string command)
        {
            await _botCommander._botBase.TestKahlaLive(_botCommander._kahlaLocation.ToString());
        }
    }
}
