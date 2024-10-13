using Aiursoft.AiurObserver.DefaultConsumers;
using Aiursoft.AiurObserver.WebSocket;
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
        var pusher = await Sdk.InitPusherAsync();
        var endpointUrl = pusher.WebSocketEndpoint;
        var socket = await endpointUrl.ConnectAsWebSocketServer();
        var socketStage = new MessageStageLast<string>();
        var subscription = socket.Subscribe(socketStage);
        await Task.Factory.StartNew(() => socket.Listen());
        await Sdk.PushTestAsync();
        await Task.Delay(500);
        Assert.IsTrue(socketStage.Stage?.Contains("message"));
        subscription.Unsubscribe();
    }
}