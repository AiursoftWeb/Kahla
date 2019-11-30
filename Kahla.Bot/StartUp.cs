using Aiursoft.Pylon;
using Aiursoft.Pylon.Interfaces;
using Kahla.Bot.Bots;
using Kahla.Bot.Core;
using Kahla.SDK.Models;
using Kahla.SDK.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Threading.Tasks;

namespace Kahla.Bot
{
    public class StartUp : IScopedDependency
    {
        private readonly BotListener _botListener;
        private readonly BotCommander _botCommander;
        private readonly EchoBot _echoBot;

        public StartUp(
            BotListener botListener,
            BotCommander botCommander,
            EchoBot bot)
        {
            _botListener = botListener;
            _botCommander = botCommander;
            _echoBot = bot;
        }

        public static IServiceScope ConfigureServices()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };

            var services = new ServiceCollection();
            services.AddAiurDependencies<KahlaUser>("Kahla");
            services.AddSingleton<KahlaLocation>();
            services.AddSingleton<SingletonHTTP>();
            services.AddScoped<HomeService>();
            services.AddScoped<AuthService>();
            services.AddScoped<FriendshipService>();
            services.AddScoped<ConversationService>();
            services.AddTransient<AES>();
            Console.Clear();

            return services.BuildServiceProvider()
                 .GetService<IServiceScopeFactory>()
                 .CreateScope();
        }

        public async Task Start()
        {
            var listenTask = await _botListener
                .WithBot(_echoBot)
                .Start();

            Console.WriteLine("Bot started. Waitting for commands.");
            await Task.WhenAll(listenTask, _botCommander.Command());
        }
    }
}
