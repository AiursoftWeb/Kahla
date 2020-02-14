using Aiursoft.Scanner;
using Aiursoft.Scanner.Interfaces;
using Kahla.SDK.Abstract;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace Kahla.Bot
{
    public class StartUp : IScopedDependency
    {
        public BotBase Bot { get; set; }

        public StartUp(BotSelector botConfigurer)
        {
            Bot = botConfigurer.SelectBot();
        }

        public static IServiceProvider ConfigureServices()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };

            return new ServiceCollection()
                .AddScannedDependencies()
                .AddBots();
        }
    }
}
