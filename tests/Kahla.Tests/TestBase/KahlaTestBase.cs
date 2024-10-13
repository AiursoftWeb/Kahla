using Aiursoft.CSTools.Tools;
using Aiursoft.DbTools;
using Aiursoft.Kahla.SDK;
using Aiursoft.Kahla.SDK.Services;
using Aiursoft.Kahla.Server;
using Aiursoft.Kahla.Server.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Aiursoft.WebTools.Extends;

namespace Aiursoft.Kahla.Tests.TestBase;

public abstract class KahlaTestBase
{
    protected IHost? Server;
    protected readonly int Port;
    protected readonly KahlaServerAccess Sdk;

    protected KahlaTestBase()
    {
        Port = Network.GetAvailablePort();
        var endpointUrl = $"http://localhost:{Port}";

        var services = new ServiceCollection();
        services.AddKahlaService(endpointUrl);
        var serviceProvider = services.BuildServiceProvider();
        Sdk = serviceProvider.GetRequiredService<KahlaServerAccess>();
    }

    [TestInitialize]
    public async Task CreateServer()
    {
        Server = await AppAsync<Startup>([], port: Port);
        await Server.UpdateDbAsync<KahlaDbContext>(UpdateMode.RecreateThenUse);
        await Server.StartAsync();
    }

    [TestCleanup]
    public async Task CleanServer()
    {
        if (Server == null) return;
        await Server.StopAsync();
        Server.Dispose();
    }

}