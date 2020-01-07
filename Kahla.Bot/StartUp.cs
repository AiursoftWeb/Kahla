using Aiursoft.XelNaga.Interfaces;
using Aiursoft.XelNaga.Tools;
using Kahla.SDK.Abstract;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Threading.Tasks;

namespace Kahla.Bot
{
    public class StartUp : IScopedDependency
    {
        private readonly BotBase _bot;

        public StartUp(BotFactory botFactory)
        {
            _bot = botFactory.SelectBot();
        }

        public static IServiceScope ConfigureServices()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };

            return new ServiceCollection()
                .AddAccessiableDependencies()
                .AddBots()
                .BuildServiceProvider()
                .GetService<IServiceScopeFactory>()
                .CreateScope();
        }

        public Task Start() => _bot.Start();
    }
}
