using Kahla.SDK.Abstract;
using System.Threading.Tasks;

namespace Kahla.SDK.CommandHandlers
{
    [CommandHandler("logout")]
    public class LogoutCommandHandler : CommandHandlerBase
    {
        public LogoutCommandHandler(BotCommander botCommander) : base(botCommander)
        {

        }

        public async override Task Execute(string command)
        {
            await _botCommander._botBase.LogOff();
            _botCommander._botLogger.LogWarning($"Successfully log out. Use command:`reboot` to reconnect.");
        }
    }
}
