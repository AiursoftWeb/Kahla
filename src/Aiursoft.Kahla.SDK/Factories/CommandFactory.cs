using Aiursoft.Kahla.SDK.Abstract;
using Microsoft.Extensions.DependencyInjection;

namespace Aiursoft.Kahla.SDK.Factories
{
    public class CommandFactory<T> where T : BotBase
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private BotHost<T> _instance;

        public CommandFactory(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public CommandFactory<T> InjectHost(BotHost<T> instance)
        {
            _instance = instance;
            return this;
        }

        public ICommandHandler<T> ProduceHandler(string command)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var handlers = scope.ServiceProvider.GetRequiredService<IEnumerable<ICommandHandler<T>>>();
            foreach (var handler in handlers)
            {
                handler.InjectHost(_instance);
                if (handler.CanHandle(command.ToLower().Trim()))
                {
                    return handler;
                }
            }
            return null;
        }
    }
}
