using Aiursoft.AiurProtocol;
using Microsoft.Extensions.DependencyInjection;

namespace Aiursoft.Kahla.SDK.Services;

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