using Aiursoft.Pylon;
using Kahla.SDK.Models;
using Kahla.SDK.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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
            services.AddScoped<ConversationService>();
            services.AddTransient<AES>();

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };

            var scope = services.BuildServiceProvider()
                 .GetService<IServiceScopeFactory>()
                 .CreateScope();

            Console.Clear();

            var bot = scope.ServiceProvider.GetService<BotCore>();

            bot.GenerateResponse = ResponseUserMessage;
            bot.Run().Wait();
        }

        private static string ResponseUserMessage(string inputMessage)
        {
            var firstReplace = inputMessage
                .Replace("吗", "")
                .Replace('？', '！')
                .Replace('?', '!');
            if (inputMessage.Contains("?") || inputMessage.Contains("？"))
            {
                firstReplace = firstReplace.Replace("是", "又是");
            }
            else
            {
                firstReplace = firstReplace.Replace("是", "也是");
            }
            return firstReplace;
        }
    }
}
