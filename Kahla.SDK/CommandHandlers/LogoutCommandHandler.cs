//using Kahla.SDK.Abstract;
//using Kahla.SDK.Services;
//using System.Threading.Tasks;

//namespace Kahla.SDK.CommandHandlers
//{
//    public class LogoutCommandHandler : CommandHandlerBase
//    {
//        private readonly BotLogger _botLogger;

//        public LogoutCommandHandler(BotLogger botLogger)
//        {
//            _botLogger = botLogger;
//        }

//        public override bool CanHandle(string command)
//        {
//            return command.StartsWith("logout");
//        }

//        public async override Task Execute(string command)
//        {
//            await _botCommander.BotHost.LogOff();
//            _botLogger.LogWarning($"Successfully log out. Use command:`reboot` to reconnect.");
//        }
//    }
//}
