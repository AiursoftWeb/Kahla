using Aiursoft.AiurObserver;
using Aiursoft.AiurObserver.DefaultConsumers;
using Aiursoft.AiurObserver.WebSocket;
using Aiursoft.CSTools.Tools;
using Aiursoft.DbTools;
using Aiursoft.Kahla.SDK;
using Aiursoft.Kahla.SDK.Events;
using Aiursoft.Kahla.SDK.Services;
using Aiursoft.Kahla.Server;
using Aiursoft.Kahla.Server.Data;
using Aiursoft.WebTools.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Aiursoft.WebTools.Extends;

namespace Aiursoft.Kahla.Tests.TestBase;

public abstract class KahlaTestBase
{
    private readonly int _port;
    private readonly List<string> _users = [];
    protected readonly KahlaServerAccess Sdk;
    protected IHost? Server;

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
    public async Task TestInitialize()
    {
        Server = await AppAsync<Startup>([], port: _port);
        await Server.UpdateDbAsync<KahlaRelationalDbContext>(UpdateMode.RecreateThenUse);

        var serverConfig = Server.Services.GetRequiredService<IConfiguration>();
        var storePath = serverConfig.GetSection("Storage:Path").Value;
        var dbPath = Path.Combine(storePath!, "MessagesDbFiles");
        if (Directory.Exists(dbPath))
        {
            Directory.Delete(dbPath, true);
        }

        Directory.CreateDirectory(dbPath);
        LimitPerMin.GlobalEnabled = false;

        await Server.Services.GetRequiredService<QuickMessageAccess>().LoadAsync();
        await Server.StartAsync();
    }

    [TestCleanup]
    public async Task CleanServer()
    {
        if (Server == null) return;
        await Server.StopAsync();
        Server.Dispose();
    }

    protected async Task RunUnderUser(string userId, Func<Task> action)
    {
        if (!_users.Contains(userId))
        {
            await Sdk.RegisterAsync($"{userId}@domain.com", "password");
            _users.Add(userId);
        }
        else
        {
            await Sdk.SignInAsync($"{userId}@domain.com", "password");
        }

        await action();
        await Sdk.SignoutAsync();
    }

    protected async Task<KahlaEvent> RunAndGetEvent(Func<Task> action)
    {
        ISubscription? subscription = null;
        ObservableWebSocket? wsObject = null;
        try
        {
            var ws = await Sdk.InitThreadsWebSocketAsync();
            wsObject = await ws.WebSocketEndpoint.ConnectAsWebSocketServer();
            var socketStage = new MessageStageLast<KahlaEvent>();
            subscription = wsObject
                .Map(JsonTools.DeseralizeKahlaEvent)
                .Subscribe(socketStage);
            await Task.Factory.StartNew(() => wsObject.Listen());

            await action();

            return await socketStage.WaitOneEvent();
        }
        finally
        {
            subscription?.Unsubscribe();
            await (wsObject?.Close() ?? Task.CompletedTask);
        }
    }
}