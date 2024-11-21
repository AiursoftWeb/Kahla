using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Services;
using Aiursoft.Kahla.Tests.TestBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aiursoft.Kahla.Tests.SdkTests;

[TestClass]
public class MessagesTests : KahlaTestBase
{
    [TestMethod]
    public async Task TestOfflineAndReconnect()
    {
        var threadId = 0;
        string ws1 = string.Empty, ws2 = string.Empty, ws3 = string.Empty;
        string ui1 = string.Empty, ui2 = string.Empty, ui3 = string.Empty;
        await RunUnderUser("wsuser1", async () =>
        {
            var user = await Sdk.MeAsync();
            ui1 = user.User.Id;
            var result = await Sdk.CreateFromScratchAsync(
                name: "Sample thread",
                allowSearchByName: true,
                allowMemberSoftInvitation: true,
                allowMembersSendMessages: true,
                allowDirectJoinWithoutInvitation: true,
                allowMembersEnlistAllMembers: true);
            threadId = result.NewThreadId;
            var ws = await Sdk.InitThreadWebSocketAsync(threadId);
            ws1 = ws.WebSocketEndpoint;
        });
        await RunUnderUser("wsuser2", async () =>
        {
            var user = await Sdk.MeAsync();
            ui2 = user.User.Id;
            await Sdk.DirectJoinAsync(threadId);
            var ws = await Sdk.InitThreadWebSocketAsync(threadId);
            ws2 = ws.WebSocketEndpoint;
        });
        await RunUnderUser("wsuser3", async () =>
        {
            var user = await Sdk.MeAsync();
            ui3 = user.User.Id;
            await Sdk.DirectJoinAsync(threadId);
            var ws = await Sdk.InitThreadWebSocketAsync(threadId);
            ws3 = ws.WebSocketEndpoint;
        });
        Assert.IsNotNull(ws1);
        Assert.IsNotNull(ws2);
        Assert.IsNotNull(ws3);

        var repo1 = await new KahlaMessagesRepo(ws1).ConnectAndMonitor();
        var repo2 = await new KahlaMessagesRepo(ws2).ConnectAndMonitor();

        // Client 1 takes action.
        await repo1.Send(new ChatMessage
        {
            Content = "Hello, world!",
            SenderId = Guid.Parse(ui1)
        });

        // Reflect to client 2.
        Assert.AreEqual(1, repo2.GetAllMessages().Count());
        Assert.AreEqual("Hello, world!", repo2.Head()?.Item.Content);

        // Prepare client 3.
        var repo3 = await new KahlaMessagesRepo(ws3).ConnectAndMonitor();
        await Task.Delay(100);

        // Client 3 gets the message.
        Assert.AreEqual(1, repo3.GetAllMessages().Count());
        Assert.AreEqual("Hello, world!", repo3.Head()?.Item.Content);

        // Client 2 drop.
        await repo2.Disconnect();

        // Client 3 takes action.
        await repo3.Send(new ChatMessage
        {
            Content = "Hello, world! 2",
            SenderId = Guid.Parse(ui3)
        });

        // Reflect to client 1.
        Assert.AreEqual(2, repo1.GetAllMessages().Count());
        Assert.AreEqual("Hello, world! 2", repo1.Head()?.Item.Content);

        // Not reflect to client 2.
        Assert.AreEqual(1, repo2.GetAllMessages().Count());

        // Client 2 commit locally (Not sync to server).
        repo2.CommitOnly(new ChatMessage
        {
            Content = "Hello, world! 3",
            SenderId = Guid.Parse(ui2)
        });
        repo2.CommitOnly(new ChatMessage
        {
            Content = "Hello, world! 4",
            SenderId = Guid.Parse(ui2)
        });

        // Not reflect to client 1 and client 3.
        Assert.AreEqual(2, repo1.GetAllMessages().Count());
        Assert.AreEqual(3, repo2.GetAllMessages().Count());
        Assert.AreEqual(2, repo3.GetAllMessages().Count());
        Assert.AreEqual("Hello, world! 2", repo1.Head()?.Item.Content);
        Assert.AreEqual("Hello, world! 4", repo2.Head()?.Item.Content);
        Assert.AreEqual("Hello, world! 2", repo3.Head()?.Item.Content);

        // Client 2 reconnect.
        await repo2.ConnectAndMonitor();
        await Task.Delay(100);

        // All has 4 messages: Hw, Hw2, Hw3, Hw4.
        Assert.AreEqual(4, repo1.GetAllMessages().Count());
        Assert.AreEqual(4, repo2.GetAllMessages().Count());
        Assert.AreEqual(4, repo3.GetAllMessages().Count());
        Assert.AreEqual("Hello, world! 4", repo1.Head()?.Item.Content);
        Assert.AreEqual("Hello, world! 4", repo2.Head()?.Item.Content);
        Assert.AreEqual("Hello, world! 4", repo3.Head()?.Item.Content);
        Assert.AreEqual(Guid.Parse(ui2), repo1.Head()?.Item.SenderId);
        Assert.AreEqual(Guid.Parse(ui2), repo2.Head()?.Item.SenderId);
        Assert.AreEqual(Guid.Parse(ui2), repo3.Head()?.Item.SenderId);
        
        // Clean
        await repo1.Disconnect();
        await repo2.Disconnect();
        await repo3.Disconnect();
    }
}