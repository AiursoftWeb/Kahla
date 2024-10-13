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
    public async Task GetMyInfoUnauthorized()
    {
        await _sdk.RegisterAsync("user4@domain.com", "password");
        await _sdk.SignoutAsync();
        
        try
        {
            await _sdk.MeAsync();
            Assert.Fail();
        }
        catch (AiurUnexpectedServerResponseException e)
        {
            Assert.IsTrue(e.Message.StartsWith("You are unauthorized to access this API."));
        }
    }

    [TestMethod]
    public async Task PatchMyInfo()
    {
        await _sdk.RegisterAsync("user5@domain.com", "password");
        var me = await _sdk.MeAsync();
        Assert.AreEqual("user5", me.User.NickName);
        
        await _sdk.UpdateMeAsync(themeId: 1, listInSearchResult: false, nickName: "new nick name!");
        var me2 = await _sdk.MeAsync();
        Assert.AreEqual("new nick name!", me2.User.NickName);
        Assert.AreEqual(1, me2.PrivateSettings.ThemeId);
        Assert.IsFalse(me2.PrivateSettings.AllowSearchByName);
    }
    
    [TestMethod]
    public async Task GetMyDevices()
    {
        await _sdk.RegisterAsync("user6@domain.com", "password");
        var devices = await _sdk.MyDevicesAsync();
        Assert.AreEqual(0, devices.Items?.Count);
    }
    
    [TestMethod]
    public async Task AddAndGetMyDevices()
    {
        await _sdk.RegisterAsync("user7@domain.com", "password");
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
        await _sdk.RegisterAsync("user8@domain.com", "password");
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
        await _sdk.RegisterAsync("user9@domain.com", "password");
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
        await _sdk.RegisterAsync("user10@domain.com", "password");
        var addResponse = await _sdk.AddDeviceAsync("device1", "auth", "endpoint://test_endpoint", "p256dh");
        Assert.AreEqual(Code.JobDone, addResponse.Code);
        await _sdk.PushTestAsync();
    }

    [TestMethod]
    public async Task AddMySelfAsContactTest()
    {
        // Register
        await _sdk.RegisterAsync("user12@domain.com", "password");
        
        // No contacts.
        var myContacts = await _sdk.MineAsync(take: 2);
        Assert.AreEqual(0, myContacts.KnownContacts.Count);

        // Search me.
        var searchResult = await _sdk.SearchEverythingAsync("user12");
        Assert.AreEqual(Code.ResultShown, searchResult.Code);
        Assert.AreEqual(1, searchResult.Users.Count);
        Assert.AreEqual("user12", searchResult.Users.First().User.NickName);
        Assert.AreEqual(false, searchResult.Users.First().IsKnownContact);
        
        // Add me as a contact.
        var addResult = await _sdk.AddContactAsync(searchResult.Users.First().User.Id);
        Assert.AreEqual(Code.JobDone, addResult.Code);

        // Add again will fail.
        try
        {
            await _sdk.AddContactAsync(searchResult.Users.First().User.Id);
            Assert.Fail();
        }
        catch
        {
            // ignored
        }

        // I should have one contact now.
        var myContacts2 = await _sdk.MineAsync(take: 2);
        Assert.AreEqual(1, myContacts2.KnownContacts.Count);
        Assert.AreEqual("user12", myContacts2.KnownContacts.First().User.NickName);
        Assert.AreEqual(true, myContacts2.KnownContacts.First().IsKnownContact);

        // Remove me as a contact.
        var removeResult = await _sdk.RemoveContactAsync(searchResult.Users.First().User.Id);
        Assert.AreEqual(Code.JobDone, removeResult.Code);
        
        // Remove again will fail.
        try
        {
            await _sdk.RemoveContactAsync(searchResult.Users.First().User.Id);
            Assert.Fail();
        }
        catch
        {
            // ignored
        }
        
        // I should have no contact now.
        var myContacts3 = await _sdk.MineAsync(take: 2);
        Assert.AreEqual(0, myContacts3.KnownContacts.Count);
    }

    [TestMethod]
    public async Task GetMyDetailsTest()
    {
        // Register
        await _sdk.RegisterAsync("user13@domain.com", "password");
        var searchResult = await _sdk.SearchEverythingAsync("user13");
        Assert.AreEqual(Code.ResultShown, searchResult.Code);
        Assert.AreEqual(1, searchResult.Users.Count);
        
        var details = await _sdk.UserDetailAsync(searchResult.Users.First().User.Id);
        Assert.AreEqual("user13", details.SearchedUser.User.NickName);
    }

    [TestMethod]
    public async Task ReportTwiceTest()
    {
        // Register bad guy.
        await _sdk.RegisterAsync("bad@domain.com", "password");
        
        // Register
        await _sdk.RegisterAsync("user14@domain.com", "password");
        
        // Search bad guy.
        var searchResult = await _sdk.SearchEverythingAsync("bad");
        
        // Report
        var reportResult = await _sdk.ReportUserAsync(searchResult.Users.First().User.Id, "reason1");
        Assert.AreEqual(Code.JobDone, reportResult.Code);
        
        // Report again should fail.
        try
        {
            await _sdk.ReportUserAsync(searchResult.Users.First().User.Id, "reason2");
            Assert.Fail();
        }
        catch
        {
            // ignored
        }
    }
    
    [TestMethod]
    public async Task WebSocketPushTest()
    {
        await _sdk.RegisterAsync("user11@domain.com", "password");
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