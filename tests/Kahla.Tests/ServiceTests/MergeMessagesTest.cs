using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Services;
using Aiursoft.Kahla.Tests.TestBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aiursoft.Kahla.Tests;

[TestClass]
public class MergeMessagesTest : KahlaTestBase
{
    [TestMethod]
    public async Task TestDeviceOfflineAndOnline()
    {
        int threadId = 0;
        string ws1WebSocket = string.Empty;
        string ws2WebSocket = string.Empty;
        await RunUnderUser("kahla-user1", async () =>
        {
            var thread = await Sdk.CreateFromScratchAsync(
                "My thread",
                allowSearchByName: true,
                allowDirectJoinWithoutInvitation: true,
                allowMemberSoftInvitation: true,
                allowMembersSendMessages: true,
                allowMembersEnlistAllMembers: true);
            threadId = thread.NewThreadId;
            ws1WebSocket = (await Sdk.InitThreadWebSocketAsync(threadId)).WebSocketEndpoint;
        });
        await RunUnderUser("kahla-user2", async () =>
        {
            await Sdk.DirectJoinAsync(threadId);
            ws2WebSocket = (await Sdk.InitThreadWebSocketAsync(threadId)).WebSocketEndpoint;
        });

        var repo1 = await new KahlaMessagesRepo(ws1WebSocket).ConnectAndMonitor();
        var repo2 = await new KahlaMessagesRepo(ws2WebSocket).ConnectAndMonitor();
        
        // User 1 send two messages.
        await repo1.Send(new ChatMessage
        {
            Content = "Hello!",
            Preview = "p1"
        });
        await repo1.Send(new ChatMessage
        {
            Content = "World!",
            Preview = "p2"
        });
        
        // User 2 get the two messages.
        Assert.AreEqual("Hello!", repo2.GetAllMessages().First().Item.Content);
        Assert.AreEqual("World!", repo2.Head()?.Item.Content);
        Assert.AreEqual("p1", repo2.GetAllMessages().First().Item.Preview);
        Assert.AreEqual("p2", repo2.Head()?.Item.Preview);
        
        // Disconnect user 2.
        await repo2.Disconnect();
        
        // User 1 and User 2 sends messages at the same time.
        // Only user 1 can send to the server.
        await repo1.Send(new ChatMessage
        {
            Content = "User 1's message",
            Preview = "p3"
        });
        repo2.CommitOnly(new ChatMessage
        {
            Content = "User 2's message",
            Preview = "p4"
        });
        
        // Reconnect user 2.
        await repo2.ConnectAndMonitor();
        await Task.Delay(100);
        
        var allUser2Messages = repo2.GetAllMessages().ToList();
        Assert.AreEqual("User 1's message", allUser2Messages[2].Item.Content);
        Assert.AreEqual("User 2's message", allUser2Messages[3].Item.Content);
        
        // Reconnect user 2.
        var allUser1Messages = repo1.GetAllMessages().ToList();
        Assert.AreEqual("User 1's message", allUser1Messages[2].Item.Content);
        Assert.AreEqual("User 2's message", allUser1Messages[3].Item.Content);
        
        // Clean up
        await repo1.Disconnect();
        await repo2.Disconnect();
    }
}