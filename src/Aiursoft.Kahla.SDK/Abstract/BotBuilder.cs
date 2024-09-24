using Aiursoft.Scanner;
using Microsoft.Extensions.DependencyInjection;

namespace Aiursoft.Kahla.SDK.Abstract
{
    public class BotBuilder
    {
        private readonly ServiceCollection _services;

        public BotBuilder()
        {
            _services = new ServiceCollection();
            _services.AddHttpClient();
            _services.AddBots();
            _services.AddLibraryDependencies();
        }

        public BotBuilder UseStartUp<T>() where T : IStartUp, new()
        {
            var starter = new T();
            starter.ConfigureServices(_services);
            starter.Configure();
            return this;
        }

        public BotHost<T> Build<T>() where T : BotBase
        {
            var serviceProvider = _services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var botHost = scope.ServiceProvider.GetRequiredService<BotHost<T>>();
            return botHost;
        }
    }
}
