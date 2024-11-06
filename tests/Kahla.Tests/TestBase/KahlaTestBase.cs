using Aiursoft.CSTools.Tools;
using Aiursoft.DbTools;
using Aiursoft.Kahla.SDK;
using Aiursoft.Kahla.SDK.Services;
using Aiursoft.Kahla.Server;
using Aiursoft.Kahla.Server.Data;
using Aiursoft.WebTools.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Aiursoft.WebTools.Extends;

namespace Aiursoft.Kahla.Tests.TestBase;

public abstract class KahlaTestBase
{
    private IHost? _server;
    private readonly int _port;
    protected readonly KahlaServerAccess Sdk;

    protected KahlaTestBase()
    {
        _port = Network.GetAvailablePort();
        var endpointUrl = $"http://localhost:{_port}";

        var services = new ServiceCollection();
        services.AddKahlaService(endpointUrl);
        var serviceProvider = services.BuildServiceProvider();
        Sdk = serviceProvider.GetRequiredService<KahlaServerAccess>();
    }

    [TestInitialize]
    public async Task CreateServer()
    {
        _server = await AppAsync<Startup>([], port: _port);
        await _server.UpdateDbAsync<KahlaDbContext>(UpdateMode.RecreateThenUse);
        await _server.StartAsync();
        
        var serverConfig = _server.Services.GetRequiredService<IConfiguration>();
        var storePath = serverConfig.GetSection("Storage:Path").Value;
        if (Directory.Exists(storePath))
        {
            Directory.Delete(storePath, true);
        }
        LimitPerMin.GlobalEnabled = false;
    }

    [TestCleanup]
    public async Task CleanServer()
    {
        if (_server == null) return;
        await _server.StopAsync();
        _server.Dispose();
    }

}