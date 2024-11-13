using Aiursoft.AiurEventSyncer.Models;
using Aiursoft.AiurEventSyncer.Remotes;
using Aiursoft.CSTools.Tools;
using Aiursoft.DbTools;
using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.Server;
using Aiursoft.Kahla.Server.Data;
using Aiursoft.Kahla.Server.Models.Entities;
using Aiursoft.Kahla.Tests.TestBase;
using Aiursoft.WebTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aiursoft.Kahla.Tests;

[TestClass]
public class MemoryLayerTests : KahlaTestBase
{
    [TestMethod]
    public async Task LoadQuickMessagesTest()
    {
        var port = Network.GetAvailablePort();
        var server = await Extends.AppAsync<Startup>([], port: port);
        await server.UpdateDbAsync<KahlaRelationalDbContext>(UpdateMode.RecreateThenUse);
        var dbContext = server
            .Services
            .GetRequiredService<IServiceScopeFactory>()
            .CreateScope()
            .ServiceProvider
            .GetRequiredService<KahlaRelationalDbContext>();

        var arrayDbContext = server
            .Services
            .GetRequiredService<ArrayDbContext>();

        // Add a user.
        var user = new KahlaUser
        {
            Email = "test@domain.com",
            NickName = "Test",
        };
        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();

        // Add a chat thread.
        var thread = new ChatThread
        {
            Name = "Test",
        };
        await dbContext.ChatThreads.AddAsync(thread);
        await dbContext.SaveChangesAsync();

        // Add the user to the thread.
        await dbContext.UserThreadRelations.AddAsync(new UserThreadRelation
        {
            UserId = user.Id,
            ThreadId = thread.Id,
        });

        arrayDbContext.AddMessage(new MessageInDatabaseEntity
        {
            Id = Guid.NewGuid(),
            ThreadId = thread.Id,
            SenderId = Guid.Parse(user.Id),
            Content = "Test",
            CreationTime = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();
        await server.Services.GetRequiredService<QuickMessageAccess>().LoadAsync();
    }

    [TestMethod]
    public async Task TestThreadsOrderingSystem()
    {
        await RunUnderUser("wsuser1", async () =>
        {
            var myId = (await Sdk.MeAsync()).User.Id;
            var result1 = await Sdk.CreateFromScratchAsync(
                name: "Sample thread 1",
                allowSearchByName: true,
                allowMemberSoftInvitation: true,
                allowMembersSendMessages: true,
                allowDirectJoinWithoutInvitation: true,
                allowMembersEnlistAllMembers: true);
            var result2 = await Sdk.CreateFromScratchAsync(
                name: "Sample thread 2",
                allowSearchByName: true,
                allowMemberSoftInvitation: true,
                allowMembersSendMessages: true,
                allowDirectJoinWithoutInvitation: true,
                allowMembersEnlistAllMembers: true);

            // My Threads should return the threads in the order of creation.
            var myThreads = await Sdk.MyThreadsAsync();
            Assert.AreEqual("Sample thread 2", myThreads.KnownThreads[0].Name);
            Assert.AreEqual("Sample thread 1", myThreads.KnownThreads[1].Name);

            // Search threads should return the threads in the order of creation.
            var searchedThreads = await Sdk.ListThreadsAsync();
            Assert.AreEqual("Sample thread 2", searchedThreads.KnownThreads[0].Name);
            Assert.AreEqual("Sample thread 1", searchedThreads.KnownThreads[1].Name);

            // Connect to the two threads.
            var ws1 = (await Sdk.InitThreadWebSocketAsync(result1.NewThreadId)).WebSocketEndpoint;
            var ws2 = (await Sdk.InitThreadWebSocketAsync(result2.NewThreadId)).WebSocketEndpoint;
            var repo1 = new Repository<ChatMessage>();
            await new WebSocketRemote<ChatMessage>(ws1)
                .AttachAsync(repo1);
            var repo2 = new Repository<ChatMessage>();
            var cn2 = await new WebSocketRemote<ChatMessage>(ws2)
                .AttachAsync(repo2);

            // Send a mew message to thread 1.
            repo1.Commit(new ChatMessage
            {
                Content = "Hello, world!",
                SenderId = Guid.Parse(myId)
            });
            await Task.Delay(1000);

            // My Threads should return the threads in the order of last message time.
            myThreads = await Sdk.MyThreadsAsync();
            Assert.AreEqual("Hello, world!", myThreads.KnownThreads[0].MessageContext.LatestMessage!.Content);
            Assert.AreEqual("Sample thread 1", myThreads.KnownThreads[0].Name);
            Assert.AreEqual("Sample thread 2", myThreads.KnownThreads[1].Name);

            // Search threads should still return the threads in the order of creation.
            searchedThreads = await Sdk.ListThreadsAsync();
            Assert.AreEqual("Sample thread 2", searchedThreads.KnownThreads[0].Name);
            Assert.AreEqual("Sample thread 1", searchedThreads.KnownThreads[1].Name);

            // Create thread 3.
            _ = await Sdk.CreateFromScratchAsync(
                name: "Sample thread 3",
                allowSearchByName: true,
                allowMemberSoftInvitation: true,
                allowMembersSendMessages: true,
                allowDirectJoinWithoutInvitation: true,
                allowMembersEnlistAllMembers: true);

            // Send a message to thread 2.
            repo2.Commit(new ChatMessage
            {
                Content = "Hello, world! 2",
                SenderId = Guid.Parse(myId)
            });
            await Task.Delay(1000);

            // Reload the threads.
            await Ensure3ThreadsCorrect();
            await Server!.Services.GetRequiredService<QuickMessageAccess>().LoadAsync();
            await Ensure3ThreadsCorrect();

            // Leave thread 2.
            await cn2.DetachAsync();
            await Sdk.DissolveThreadAsync(result2.NewThreadId);

            await Ensure2ThreadsCorrect();
        });

        async Task Ensure3ThreadsCorrect()
        {
            // My Threads should return the threads in the order of last message time.
            var myThreads = await Sdk.MyThreadsAsync();
            Assert.AreEqual(3, myThreads.KnownThreads.Count);
            // Thread 2 should be the first one.
            Assert.AreEqual("Hello, world! 2", myThreads.KnownThreads[0].MessageContext.LatestMessage!.Content);
            Assert.AreEqual("Sample thread 2", myThreads.KnownThreads[0].Name);
            // Thread 3 should be the second one.
            Assert.AreEqual(null, myThreads.KnownThreads[1].MessageContext.LatestMessage);
            Assert.AreEqual("Sample thread 3", myThreads.KnownThreads[1].Name);
            // Thread 1 should be the last one.
            Assert.AreEqual("Hello, world!", myThreads.KnownThreads[2].MessageContext.LatestMessage!.Content);
            Assert.AreEqual("Sample thread 1", myThreads.KnownThreads[2].Name);

            // Search threads should still return the threads in the order of creation.
            var searchedThreads = await Sdk.ListThreadsAsync();
            Assert.AreEqual(3, searchedThreads.KnownThreads.Count);
            Assert.AreEqual("Sample thread 3", searchedThreads.KnownThreads[0].Name);
            Assert.AreEqual("Sample thread 2", searchedThreads.KnownThreads[1].Name);
            Assert.AreEqual("Sample thread 1", searchedThreads.KnownThreads[2].Name);
        }

        async Task Ensure2ThreadsCorrect()
        {
            // My Threads should return the threads in the order of last message time.
            var myThreads = await Sdk.MyThreadsAsync();
            Assert.AreEqual(2, myThreads.KnownThreads.Count);
            Assert.AreEqual(null, myThreads.KnownThreads[0].MessageContext.LatestMessage);
            Assert.AreEqual("Sample thread 3", myThreads.KnownThreads[0].Name);
            Assert.AreEqual("Hello, world!", myThreads.KnownThreads[1].MessageContext.LatestMessage!.Content);
            Assert.AreEqual("Sample thread 1", myThreads.KnownThreads[1].Name);

            // Search threads should still return the threads in the order of creation.
            var searchedThreads = await Sdk.ListThreadsAsync();
            Assert.AreEqual(2, searchedThreads.KnownThreads.Count);
            Assert.AreEqual("Sample thread 3", searchedThreads.KnownThreads[0].Name);
            Assert.AreEqual("Sample thread 1", searchedThreads.KnownThreads[1].Name);
        }
    }
}