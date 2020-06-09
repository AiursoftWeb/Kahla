using Kahla.Bot.Services;
using Kahla.SDK.Abstract;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Kahla.Bot
{
    public class StartUp : IStartUp
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<BingTranslator>();
        }

        public void Configure()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };
        }
    }
}
