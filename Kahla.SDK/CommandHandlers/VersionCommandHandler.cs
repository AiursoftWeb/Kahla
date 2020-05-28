using Kahla.SDK.Abstract;
using Kahla.SDK.Services;
using System.Threading.Tasks;

namespace Kahla.SDK.CommandHandlers
{
    public class VersionCommandHandler<T>: ICommandHandler<T> where T : BotBase
    {
        private BotHost<T> _botHost;
        private readonly KahlaLocation _kahlaLocation;

        public VersionCommandHandler(
            KahlaLocation kahlaLocation)
        {
            _kahlaLocation = kahlaLocation;
        }

        public void InjectHost(BotHost<T> instance)
        {
            _botHost = instance;
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
