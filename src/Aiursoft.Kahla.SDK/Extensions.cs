using Aiursoft.AiurProtocol;
using Aiursoft.Kahla.SDK.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Aiursoft.Kahla.SDK;

public static class Extensions
{
    public static IServiceCollection AddKahlaService(this IServiceCollection services, string endPointUrl)
    {
        services.AddAiurProtocolClient();
        services.Configure<KahlaServerConfig>(options => options.Instance = endPointUrl);
        services.AddScoped<KahlaServerAccess>();
        return services;
    }
    
    private static readonly JsonSerializerSettings Settings = new()
    {
        TypeNameHandling = TypeNameHandling.Auto,
        DateTimeZoneHandling = DateTimeZoneHandling.Utc,
        ContractResolver = new CamelCasePropertyNamesContractResolver()
    };
    
    public static string Serialize<T>(T model)
    {
        return JsonConvert.SerializeObject(model, Settings);
    }

    public static T Deserialize<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json, Settings)!;
    }
}