using Aiursoft.Pylon;
using Kahla.SDK.Models;
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

            services.AddAiurDependencies<KahlaUser>("Kahla");
            services.AddSingleton<KahlaLocation>();
            services.AddSingleton<SingletonHTTP>();
            services.AddScoped<HomeService>();
            services.AddScoped<AuthService>();
            services.AddTransient<AES>();

            var scope = services.BuildServiceProvider()
                 .GetService<IServiceScopeFactory>()
                 .CreateScope();

            Console.Clear();

            var bot = scope.ServiceProvider.GetService<BotCore>();
            bot.Run().Wait();
        }
    }
}
