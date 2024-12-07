using Aiursoft.AiurObserver.DefaultConsumers;
using Aiursoft.AiurObserver.WebSocket;
using Aiursoft.Kahla.Server.Controllers;
using Aiursoft.Kahla.Tests.TestBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aiursoft.Kahla.Tests.SdkTests;

[TestClass]
public class WebSocketTests : KahlaTestBase
{
    [TestMethod]
    public async Task WebSocketPushTest()
    {
        await Sdk.RegisterAsync("user11@domain.com", "password");
        await Sdk.PushTestAsync();
    }
    
    [TestMethod]
    public async Task WebSocketInitWithExpiredOtpTest()
    {
        MessagesController.TokenTimeout = TimeSpan.FromSeconds(-1);

        await Sdk.RegisterAsync("userExpired@domain.com", "password");
        var pusher = await Sdk.InitThreadsWebSocketAsync();

        var endpointUrl = pusher.WebSocketEndpoint;
        var exceptionThrown = false;

        try
        {
            await endpointUrl.ConnectAsWebSocketServer();
        }
        catch (Exception)
        {
            exceptionThrown = true;
        }

        Assert.IsTrue(exceptionThrown, "应当抛出异常，因为 OTP 已经过期。");
        MessagesController.TokenTimeout = TimeSpan.FromMinutes(5);
    }
    
    [TestMethod]
    public async Task WebSocketInitWithInvalidOtpTest()
    {
        await Sdk.RegisterAsync("userInvalid@domain.com", "password");
        var pusher = await Sdk.InitThreadsWebSocketAsync();

        var invalidEndpointUrl = pusher.WebSocketEndpoint.Replace("otp=", "otp=invalid_token");

        var exceptionThrown = false;
        try
        {
            await invalidEndpointUrl.ConnectAsWebSocketServer();
        }
        catch (Exception)
        {
            exceptionThrown = true;
        }

        Assert.IsTrue(exceptionThrown, "应当抛出异常，因为提供了无效的 OTP。");
    }

    [TestMethod]
    public async Task WebSocketInitWithNonExistentUserTest()
    {
        var invalidEndpointUrl = "ws://localhost/api/messages/websocket/nonexistent_user?otp=some_fake_otp";

        var exceptionThrown = false;
        try
        {
            await invalidEndpointUrl.ConnectAsWebSocketServer();
        }
        catch (Exception)
        {
            exceptionThrown = true;
        }

        Assert.IsTrue(exceptionThrown, "应当抛出异常，因为用户不存在。");
    }
    
    [TestMethod]
    public async Task WebSocketConnectionInterruptedTest()
    {
        await Sdk.RegisterAsync("userInterrupt@domain.com", "password");
        var pusher = await Sdk.InitThreadsWebSocketAsync();
        var endpointUrl = pusher.WebSocketEndpoint;

        var socket = await endpointUrl.ConnectAsWebSocketServer();
        var socketStage = new MessageStageLast<string>();
        var subscription = socket.Subscribe(socketStage);

        bool connectionInterrupted;
        try
        {
            await socket.Close();
            await Task.Delay(500);
            connectionInterrupted = true;
        }
        catch (Exception)
        {
            connectionInterrupted = false;
        }

        Assert.IsTrue(connectionInterrupted, "WebSocket 连接应被成功中断。");

        subscription.Unsubscribe();
    }

}