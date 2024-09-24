namespace Kahla.SDK.Abstract
{
    public interface ICommandHandler<T> where T : BotBase
    {
        void InjectHost(BotHost<T> instance);
        bool CanHandle(string command);
        Task<bool> Execute(string command);
    }
}
