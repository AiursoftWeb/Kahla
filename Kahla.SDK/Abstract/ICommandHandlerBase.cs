using System.Threading.Tasks;

namespace Kahla.SDK.Abstract
{
    public interface ICommandHandler
    {
        bool CanHandle(string command);
        Task Execute(string command);
    }
}
