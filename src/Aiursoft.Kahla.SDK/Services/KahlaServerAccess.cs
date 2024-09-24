using Aiursoft.AiurProtocol;
using Aiursoft.AiurProtocol.Models;
using Aiursoft.AiurProtocol.Services;
using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Models.AddressModels;
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
    
    public async Task<AiurResponse> SignInAsync(string email, string password)
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/auth/signin",param: new {});
        var model = new AiurApiPayload(new SignInAddressModel
        {
            Email = email,
            Password = password
        });
        var result = await http.Post<AiurResponse>(url, model);
        return result;
    }
    
    public async Task<AiurResponse> RegisterAsync(string email, string password)
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/auth/register", param: new {});
        var model = new AiurApiPayload(new SignInAddressModel
        {
            Email = email,
            Password = password
        });
        var result = await http.Post<AiurResponse>(url, model);
        return result;
    }
    
    public async Task<AiurResponse> SignoutAsync(int? deviceId = null)
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/auth/signout", param: new {});
        var model = new AiurApiPayload(new SignOutAddressModel
        {
            DeviceId = deviceId
        });
        var result = await http.Post<AiurResponse>(url, model);
        return result;
    }
    
    public async Task<MeViewModel> MeAsync()
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/auth/me", param: new {});
        var result = await http.Get<MeViewModel>(url);
        return result;
    }
    
    public async Task<AiurCollection<Device>> MyDevicesAsync()
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/devices/my-devices", param: new {});
        var result = await http.Get<AiurCollection<Device>>(url);
        return result;
    }
    
    public async Task<AiurValue<int>> AddDeviceAsync(string name, string pushAuth, string pushEndpoint, string pushP256Dh)
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/devices/add-device", param: new {});
        var payload = new AiurApiPayload(new AddDeviceAddressModel
        {
            Name = name,
            PushAuth = pushAuth,
            PushEndpoint = pushEndpoint,
            PushP256Dh = pushP256Dh
        });
        var result = await http.Post<AiurValue<int>>(url, payload);
        return result;
    }
    
    public async Task<AiurResponse> DropDeviceAsync(int id)
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/devices/drop-device", param: new { id });
        var result = await http.Post<AiurResponse>(url, new AiurApiPayload(new {}));
        return result;
    }
}