using Aiursoft.AiurObserver.DefaultConsumers;
using Aiursoft.AiurObserver.WebSocket;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aiursoft.Kahla.Tests.SdkTests;

[TestClass]
public class WebSocketTests : KahlaTestBase
{
     
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