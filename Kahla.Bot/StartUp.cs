using Aiursoft.Pylon;
using Kahla.SDK.Models;
using Kahla.SDK.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace Kahla.Bot
{
    public static class StartUp
    {
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
    }
}
