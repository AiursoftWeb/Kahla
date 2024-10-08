using Aiursoft.AiurObserver.DefaultConsumers;
using Aiursoft.AiurObserver.WebSocket;
using Aiursoft.AiurProtocol.Exceptions;
using Aiursoft.AiurProtocol.Models;
using Aiursoft.CSTools.Tools;
using Aiursoft.DbTools;
using Aiursoft.Kahla.SDK;
using Aiursoft.Kahla.SDK.Services;
using Aiursoft.Kahla.Server;
using Aiursoft.Kahla.Server.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Aiursoft.WebTools.Extends;

namespace Aiursoft.Kahla.Tests;

[TestClass]
public class BasicServerTests
{
    private IHost? _server;
    private readonly int _port;
    private readonly KahlaServerAccess _sdk;

    public BasicServerTests()
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

    [TestMethod]
    public async Task TestServerInfo()
    {
        var home = await _sdk.ServerInfoAsync();
        Assert.AreEqual("Your Server Name", home.ServerName);
    }

    [TestMethod]
    public async Task Register_Signout_SignIn()
    {
        await _sdk.RegisterAsync("user1@domain.com", "password");
        await _sdk.SignoutAsync();
        await _sdk.SignInAsync("user1@domain.com", "password");
    }

    [TestMethod]
    public async Task SignInInvalid()
    {
        await _sdk.RegisterAsync("userz@domain.com", "password");
        await _sdk.SignoutAsync();
        try
        {
            await _sdk.SignInAsync("userz@domain.com", "badzzzzzzz");
            Assert.Fail();
        }
        catch (AiurUnexpectedServerResponseException e)
        {
            Assert.AreEqual("Invalid login attempt! Please check your email and password.", e.Response.Message);
        }
    }

    [TestMethod]
    public async Task SignInWhileSignedIn()
    {
        await _sdk.RegisterAsync("user2@domain.com", "password");
        try
        {
            await _sdk.SignInAsync("zzzzzzzz@domain.com", "password");
            Assert.Fail();
        }
        catch (AiurUnexpectedServerResponseException e)
        {
            Assert.AreEqual("You are already signed in!", e.Response.Message);
        }
    }
    
    [TestMethod]
    public async Task SignIn_ChangePassword_SignIn()
    {
        await _sdk.RegisterAsync("user11@domain.com", "password");
        try
        {
            await _sdk.ChangePasswordAsync("bad_password", "useless_string");
            Assert.Fail();
        }
        catch (AiurUnexpectedServerResponseException e)
        {
            Assert.AreEqual("Incorrect password.", e.Response.Message);
        }
        
        await _sdk.ChangePasswordAsync("password", "newpassword");
        await _sdk.SignoutAsync();
        try
        {
            await _sdk.SignInAsync("user11@domain.com", "password");
            Assert.Fail();
        }
        catch (AiurUnexpectedServerResponseException e)
        {
            Assert.AreEqual("Invalid login attempt! Please check your email and password.", e.Response.Message);
        }
        
        await _sdk.SignInAsync("user11@domain.com", "newpassword");
    }

    [TestMethod]
    public async Task DuplicateRegister()
    {
        await _sdk.RegisterAsync("anduin@aiursoft.com", "password");
        try
        {
            await _sdk.RegisterAsync("anduin@aiursoft.com", "password");
            Assert.Fail();
        }
        catch (AiurUnexpectedServerResponseException e)
        {
            Assert.AreEqual("Username 'anduin@aiursoft.com' is already taken.", e.Response.Message);
        }
    }

    [TestMethod]
    public async Task GetMyInfo()
    {
        await _sdk.RegisterAsync("user3@domain.com", "password");
        var me = await _sdk.MeAsync();
        Assert.AreEqual("user3", me.User.NickName);
        Assert.AreEqual("user3@domain.com", me.User.Email);
    }

    [TestMethod]
    public async Task PatchMyInfo()
    {
        await _sdk.RegisterAsync("user10@domain.com", "password");
        var me = await _sdk.MeAsync();
        Assert.AreEqual("user10", me.User.NickName);
        
        await _sdk.UpdateMeAsync(themeId: 1, listInSearchResult: false, nickName: "new nick name!");
        var me2 = await _sdk.MeAsync();
        Assert.AreEqual("new nick name!", me2.User.NickName);
        Assert.AreEqual(1, me2.ThemeId);
        Assert.IsFalse(me2.AllowSearchByName);
    }
    
    [TestMethod]
    public async Task GetMyDevices()
    {
        await _sdk.RegisterAsync("user4@domain.com", "password");
        var devices = await _sdk.MyDevicesAsync();
        Assert.AreEqual(0, devices.Items?.Count);
    }
    
    [TestMethod]
    public async Task AddAndGetMyDevices()
    {
        await _sdk.RegisterAsync("user5@domain.com", "password");
        var devices = await _sdk.MyDevicesAsync();
        Assert.AreEqual(0, devices.Items?.Count);
        
        await _sdk.AddDeviceAsync("device1", "auth", "endpoint://test_endpoint", "p256dh");
        var devices2 = await _sdk.MyDevicesAsync();
        Assert.AreEqual(1, devices2.Items?.Count);
        Assert.AreEqual("device1", devices2.Items?.First().Name);
    }

    [TestMethod]
    public async Task AddAndDropDevice()
    {
        await _sdk.RegisterAsync("user6@domain.com", "password");
        var devices = await _sdk.MyDevicesAsync();
        Assert.AreEqual(0, devices.Items?.Count);

        var addResponse = await _sdk.AddDeviceAsync("device1", "auth", "endpoint://test_endpoint", "p256dh");
        Assert.AreEqual(Code.JobDone, addResponse.Code);

        var devices2 = await _sdk.MyDevicesAsync();
        Assert.AreEqual(1, devices2.Items?.Count);

        var dropResponse = await _sdk.DropDeviceAsync(addResponse.Value);
        Assert.AreEqual(Code.JobDone, dropResponse.Code);

        var devices3 = await _sdk.MyDevicesAsync();
        Assert.AreEqual(0, devices3.Items?.Count);
    }

    [TestMethod]
    public async Task AddAndPatchDevice()
    {
        await _sdk.RegisterAsync("user7@domain.com", "password");
        var devices = await _sdk.MyDevicesAsync();
        Assert.AreEqual(0, devices.Items?.Count);

        var addResponse = await _sdk.AddDeviceAsync("device1", "auth", "endpoint://test_endpoint", "p256dh");
        Assert.AreEqual(Code.JobDone, addResponse.Code);

        var devices2 = await _sdk.MyDevicesAsync();
        Assert.AreEqual(1, devices2.Items?.Count);
        Assert.AreEqual("device1", devices2.Items?.First().Name);

        var patchResponse = await _sdk.UpdateDeviceAsync(addResponse.Value, "device2", "auth2",
            "endpoint://test_endpoint2", "p256dh2");
        Assert.AreEqual(Code.JobDone, patchResponse.Code);

        var devices3 = await _sdk.MyDevicesAsync();
        Assert.AreEqual(1, devices3.Items?.Count);
        Assert.AreEqual("device2", devices3.Items?.First().Name);
    }
    
    [TestMethod]
    public async Task PushTest()
    {
        await _sdk.RegisterAsync("user8@domain.com", "password");
        var addResponse = await _sdk.AddDeviceAsync("device1", "auth", "endpoint://test_endpoint", "p256dh");
        Assert.AreEqual(Code.JobDone, addResponse.Code);
        await _sdk.PushTestAsync();
    }

    [TestMethod]
    public async Task WebSocketPushTest()
    {
        await _sdk.RegisterAsync("user9@domain.com", "password");
        var pusher = await _sdk.InitPusherAsync();
        var endpointUrl = pusher.WebSocketEndpoint;
        var socket = await endpointUrl.ConnectAsWebSocketServer();
        var socketStage = new MessageStageLast<string>();
        var subscription = socket.Subscribe(socketStage);
        await Task.Factory.StartNew(() => socket.Listen());
        await _sdk.PushTestAsync();
        await Task.Delay(500);
        Assert.IsTrue(socketStage.Stage?.Contains("message"));
        subscription.Unsubscribe();
    }
}