using Kahla.SDK.Abstract;
using System.Threading.Tasks;

namespace Kahla.SDK.CommandHandlers
{
    [CommandHandler("logout")]
    public class LogoutCommandHandler<T> : CommandHandlerBase<T> where T : BotBase
    {
        public LogoutCommandHandler(BotCommander<T> botCommander) : base(botCommander)
        {

        }

        public async override Task Execute(string command)
        {
            await _botCommander._botHost.LogOff();
            _botCommander._botLogger.LogWarning($"Successfully log out. Use command:`reboot` to reconnect.");
        }
    }
}
