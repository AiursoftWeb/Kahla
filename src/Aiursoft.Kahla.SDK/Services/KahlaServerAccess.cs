using Aiursoft.AiurProtocol;
using Aiursoft.AiurProtocol.Services;
using Aiursoft.Kahla.SDK.Models.ViewModels;
using Microsoft.Extensions.Options;

namespace Aiursoft.Kahla.SDK.Services;

public class KahlaServerAccess(
    AiurProtocolClient http,
    IOptions<KahlaServerConfig> demoServerLocator)
{
    private readonly KahlaServerConfig _demoServerLocator = demoServerLocator.Value;

    public async Task<IndexViewModel> ServerInfoAsync()
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/", param: new {});
        var result = await http.Get<IndexViewModel>(url);
        return result;
    }
}