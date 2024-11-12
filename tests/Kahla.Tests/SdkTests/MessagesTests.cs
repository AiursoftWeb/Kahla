using Aiursoft.AiurEventSyncer.Models;
using Aiursoft.AiurEventSyncer.Remotes;
using Aiursoft.Kahla.SDK.Models;
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
        
        var repo1 = new Repository<ChatMessage>();
        await new WebSocketRemote<ChatMessage>(ws1)
            .AttachAsync(repo1);

        var repo2 = new Repository<ChatMessage>();
        var wsr2 = await new WebSocketRemote<ChatMessage>(ws2)
            .AttachAsync(repo2);

        // Client 1 takes action.
        repo1.Commit(new ChatMessage
        {
            Content = "Hello, world!",
            SenderId = Guid.Parse(ui1)
        });

        // Reflect to client 2.
        await Task.Delay(1500);
        Assert.AreEqual(1, repo2.Commits.Count);
        Assert.AreEqual("Hello, world!", repo2.Head.Item.Content);

        // Prepare client 3.
        var repo3 = new Repository<ChatMessage>();
        await new WebSocketRemote<ChatMessage>(ws3)
            .AttachAsync(repo3);

        // Client 3 gets the message.
        await Task.Delay(1500);
        Assert.AreEqual(1, repo3.Commits.Count);
        Assert.AreEqual("Hello, world!", repo3.Head.Item.Content);

        // Client 2 drop.
        await wsr2.DetachAsync();

        // Client 3 takes action.
        repo3.Commit(new ChatMessage
        {
            Content = "Hello, world! 2",
            SenderId = Guid.Parse(ui3)
        });

        // Reflect to client 1.
        await Task.Delay(1500);
        Assert.AreEqual(2, repo1.Commits.Count);
        Assert.AreEqual("Hello, world! 2", repo1.Head.Item.Content);

        // Not reflect to client 2.
        Assert.AreEqual(1, repo2.Commits.Count);

        // Client 2 commit locally (Not sync to server).
        repo2.Commit(new ChatMessage
        {
            Content = "Hello, world! 3",
            SenderId = Guid.Parse(ui2)
        });
        repo2.Commit(new ChatMessage
        {
            Content = "Hello, world! 4",
            SenderId = Guid.Parse(ui2)
        });

        // Not reflect to client 1 and client 3.
        await Task.Delay(1500);
        Assert.AreEqual(2, repo1.Commits.Count);
        Assert.AreEqual(3, repo2.Commits.Count);
        Assert.AreEqual(2, repo3.Commits.Count);
        Assert.AreEqual("Hello, world! 2", repo1.Head.Item.Content);
        Assert.AreEqual("Hello, world! 4", repo2.Head.Item.Content);
        Assert.AreEqual("Hello, world! 2", repo3.Head.Item.Content);
        
        // Client 2 reconnect.
        await wsr2.AttachAsync(repo2);

        // All has 3 messages: Hw, Hw2, Hw3
        await Task.Delay(1500);
        Assert.AreEqual(4, repo1.Commits.Count);
        Assert.AreEqual(4, repo2.Commits.Count);
        Assert.AreEqual(4, repo3.Commits.Count);
        Assert.AreEqual("Hello, world! 4", repo1.Head.Item.Content);
        Assert.AreEqual("Hello, world! 4", repo2.Head.Item.Content);
        Assert.AreEqual("Hello, world! 4", repo3.Head.Item.Content);
        Assert.AreEqual(Guid.Parse(ui2), repo1.Head.Item.SenderId);
        Assert.AreEqual(Guid.Parse(ui2), repo2.Head.Item.SenderId);
        Assert.AreEqual(Guid.Parse(ui2), repo3.Head.Item.SenderId);
    }

    private async Task RunUnderUser(string userId, Func<Task> action)
    {
        try
        {
            await Sdk.RegisterAsync($"{userId}@domain.com", "password");
        }
        catch
        {
            await Sdk.SignInAsync($"{userId}@domain.com", "password");
        }

        await action();
        await Sdk.SignoutAsync();
    }
}