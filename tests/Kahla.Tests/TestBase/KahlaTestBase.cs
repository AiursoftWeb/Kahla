using Aiursoft.CSTools.Tools;
using Aiursoft.DbTools;
using Aiursoft.Kahla.SDK;
using Aiursoft.Kahla.SDK.Services;
using Aiursoft.Kahla.Server;
using Aiursoft.Kahla.Server.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Aiursoft.WebTools.Extends;

namespace Aiursoft.Kahla.Tests;

public abstract class KahlaTestBase
{
    protected IHost? _server;
    protected readonly int _port;
    protected readonly KahlaServerAccess _sdk;

    protected KahlaTestBase()
    {
        _port = Network.GetAvailablePort();
        var endpointUrl = $"http://localhost:{_port}";

        var services = new ServiceCollection();
        services.AddKahlaService(endpointUrl);
        var serviceProvider = services.BuildServiceProvider();
        _sdk = serviceProvider.GetRequiredService<KahlaServerAccess>();
    }

    [TestInitialize]
    public async Task CreateServer()
    {
        _server = await AppAsync<Startup>([], port: _port);
        await _server.UpdateDbAsync<KahlaDbContext>(UpdateMode.RecreateThenUse);
        await _server.StartAsync();
    }

    [TestCleanup]
    public async Task CleanServer()
    {
        if (_server == null) return;
        await _server.StopAsync();
        _server.Dispose();
    }

}