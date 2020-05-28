using System.Threading.Tasks;

namespace Kahla.SDK.Abstract
{
    public interface ICommandHandler<T> where T : BotBase
    {
        void InjectHost(BotHost<T> instance);
        bool CanHandle(string command);
        Task Execute(string command);
    }
}
