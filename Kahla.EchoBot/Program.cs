using Aiursoft.Pylon;
using Kahla.SDK.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Kahla.EchoBot
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Kahla example bot...");
            Console.ForegroundColor = ConsoleColor.Black;
            var services = new ServiceCollection();

            services.AddAiurDependencies("Kahla");
            services.AddScoped<HomeService>();
            services.AddSingleton<KahlaLocation>();

            var scope = services.BuildServiceProvider()
                 .GetService<IServiceScopeFactory>()
                 .CreateScope();

            var bot = scope.ServiceProvider.GetService<BotCore>();

            bot.Run().Wait();
        }
    }
}
