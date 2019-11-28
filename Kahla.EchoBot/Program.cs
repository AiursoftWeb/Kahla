using Aiursoft.Pylon;
using Kahla.SDK.Events;
using Kahla.SDK.Models;
using Kahla.SDK.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace Kahla.EchoBot
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Kahla example bot...");

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

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };

            var scope = services.BuildServiceProvider()
                 .GetService<IServiceScopeFactory>()
                 .CreateScope();

            var bot = scope.ServiceProvider.GetService<BotCore>();

            bot.GenerateResponse = ResponseUserMessage;
            bot.GenerateFriendRequestResult = ResponseFriendRequest;

            bot.Run().Wait();
        }

        private static string ResponseUserMessage(string inputMessage, NewMessageEvent eventContext, Message messageContext)
        {
            if (!eventContext.Muted)
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
            else
            {
                return string.Empty;
            }
        }

        private static bool ResponseFriendRequest(NewFriendRequestEvent arg)
        {
            return true;
        }
    }
}
