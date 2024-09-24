using Aiursoft.AiurProtocol;
using Aiursoft.Kahla.SDK.Services;
using Microsoft.Extensions.DependencyInjection;

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
}