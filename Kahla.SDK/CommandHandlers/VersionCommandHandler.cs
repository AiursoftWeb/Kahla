using Kahla.SDK.Abstract;
using Kahla.SDK.Services;
using System.Threading.Tasks;

namespace Kahla.SDK.CommandHandlers
{
    public class VersionCommandHandler<T>: ICommandHandler where T : BotBase
    {
        private readonly BotHost<T> _botHost;
        private readonly KahlaLocation _kahlaLocation;

        public VersionCommandHandler(
            BotHost<T> botHost,
            KahlaLocation kahlaLocation)
        {
            _botHost = botHost;
            _kahlaLocation = kahlaLocation;
        }

        public  bool CanHandle(string command)
        {
            return command.StartsWith("version");
        }

        public async  Task Execute(string command)
        {
            await _botHost.TestKahlaLive(_kahlaLocation.ToString());
        }
    }
}
