using Aiursoft.CSTools.Tools;
using Aiursoft.DbTools;
using Aiursoft.Kahla.SDK.Events;
using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Services;
using Aiursoft.Kahla.Server;
using Aiursoft.Kahla.Server.Data;
using Aiursoft.Kahla.Server.Models.Entities;
using Aiursoft.Kahla.Server.Services.Push.WebPush;
using Aiursoft.Kahla.Tests.TestBase;
using Aiursoft.WebTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aiursoft.Kahla.Tests.ServiceTests;

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
            var searchedThreads = await Sdk.SearchThreadsAsync();
            Assert.AreEqual("Sample thread 2", searchedThreads.KnownThreads[0].Name);
            Assert.AreEqual("Sample thread 1", searchedThreads.KnownThreads[1].Name);

            // Connect to the two threads.
            var ws1 = (await Sdk.InitThreadWebSocketAsync(result1.NewThreadId)).WebSocketEndpoint;
            var ws2 = (await Sdk.InitThreadWebSocketAsync(result2.NewThreadId)).WebSocketEndpoint;
            var repo1 = await new KahlaMessagesRepo(ws1).ConnectAndMonitor();
            var repo2 = await new KahlaMessagesRepo(ws2).ConnectAndMonitor();

            // Send a mew message to thread 1.
            await repo1.Send(new ChatMessage
            {
                Content = "Hello, world!",
                Preview = "Hello, world!",
                SenderId = Guid.Parse(myId)
            });

            // My Threads should return the threads in the order of last message time.
            myThreads = await Sdk.MyThreadsAsync();
            Assert.AreEqual("Hello, world!", myThreads.KnownThreads[0].MessageContext.LatestMessage!.Preview);
            Assert.AreEqual("Sample thread 1", myThreads.KnownThreads[0].Name);
            Assert.AreEqual("Sample thread 2", myThreads.KnownThreads[1].Name);

            // Search threads should still return the threads in the order of creation.
            searchedThreads = await Sdk.SearchThreadsAsync();
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
            await repo2.Send(new ChatMessage
            {
                Content = "Hello, world! 2",
                Preview = "Hello, world! 2",
                SenderId = Guid.Parse(myId)
            });

            // Reload the threads.
            await Ensure3ThreadsCorrect();
            await Server!.Services.GetRequiredService<QuickMessageAccess>().LoadAsync();
            await Ensure3ThreadsCorrect();

            // Leave thread 2.
            await repo2.Disconnect();
            await Sdk.DissolveThreadAsync(result2.NewThreadId);

            await Ensure2ThreadsCorrect();
            await repo1.Disconnect();
        });
        return;

        async Task Ensure3ThreadsCorrect()
        {
            // My Threads should return the threads in the order of last message time.
            var myThreads = await Sdk.MyThreadsAsync();
            Assert.AreEqual(3, myThreads.KnownThreads.Count);
            // Thread 2 should be the first one.
            Assert.AreEqual("Hello, world! 2", myThreads.KnownThreads[0].MessageContext.LatestMessage!.Preview);
            Assert.AreEqual("Sample thread 2", myThreads.KnownThreads[0].Name);
            // Thread 3 should be the second one.
            Assert.AreEqual(null, myThreads.KnownThreads[1].MessageContext.LatestMessage);
            Assert.AreEqual("Sample thread 3", myThreads.KnownThreads[1].Name);
            // Thread 1 should be the last one.
            Assert.AreEqual("Hello, world!", myThreads.KnownThreads[2].MessageContext.LatestMessage!.Preview);
            Assert.AreEqual("Sample thread 1", myThreads.KnownThreads[2].Name);

            // Search threads should still return the threads in the order of creation.
            var searchedThreads = await Sdk.SearchThreadsAsync();
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
            Assert.AreEqual("Hello, world!", myThreads.KnownThreads[1].MessageContext.LatestMessage!.Preview);
            Assert.AreEqual("Sample thread 1", myThreads.KnownThreads[1].Name);

            // Search threads should still return the threads in the order of creation.
            var searchedThreads = await Sdk.SearchThreadsAsync();
            Assert.AreEqual(2, searchedThreads.KnownThreads.Count);
            Assert.AreEqual("Sample thread 3", searchedThreads.KnownThreads[0].Name);
            Assert.AreEqual("Sample thread 1", searchedThreads.KnownThreads[1].Name);

            var myThreadsSkip1Take1 = await Sdk.MyThreadsAsync(skipTillThreadId: myThreads.KnownThreads[0].Id, take: 1);
            Assert.AreEqual(1, myThreadsSkip1Take1.KnownThreads.Count);
            Assert.AreEqual("Hello, world!", myThreadsSkip1Take1.KnownThreads[0].MessageContext.LatestMessage!.Preview);
            Assert.AreEqual("Sample thread 1", myThreadsSkip1Take1.KnownThreads[0].Name);
        }
    }

    [TestMethod]
    public async Task TestThreadsUserKicked()
    {
        var thread1Id = 0;
        var user1Ws = string.Empty;
        var user2Ws = string.Empty;
        var user1Id = Guid.Empty;
        var user2Id = Guid.Empty;

        await RunUnderUser("wsuser1", async () =>
        {
            var result1 = await Sdk.CreateFromScratchAsync(
                name: "Sample thread 1",
                allowSearchByName: true,
                allowMemberSoftInvitation: true,
                allowMembersSendMessages: true,
                allowDirectJoinWithoutInvitation: true,
                allowMembersEnlistAllMembers: true);
            thread1Id = result1.NewThreadId;
            user1Ws = (await Sdk.InitThreadWebSocketAsync(thread1Id)).WebSocketEndpoint;
            user1Id = Guid.Parse((await Sdk.MeAsync()).User.Id);
        });
        await RunUnderUser("wsuser2", async () =>
        {
            user2Id = Guid.Parse((await Sdk.MeAsync()).User.Id);
            await Sdk.DirectJoinAsync(thread1Id);
            user2Ws = (await Sdk.InitThreadWebSocketAsync(thread1Id)).WebSocketEndpoint;
        });

        var repo1 = await new KahlaMessagesRepo(user1Ws).ConnectAndMonitor();
        var repo2 = await new KahlaMessagesRepo(user2Ws).ConnectAndMonitor();

        // User 1 sends a message. Reflect to user 2.
        await repo1.Send(new ChatMessage
        {
            Content = "Hello, world!",
            Preview = "Hello, world!",
            SenderId = user1Id
        });

        // User 2 should receive the message.
        Assert.AreEqual("Hello, world!", repo2.Head()?.Item.Content);

        // User 2 sends a message. Reflect to user 1.
        await repo2.Send(new ChatMessage
        {
            Content = "Hello, world! 2",
            Preview = "Hello, world! 2",
            SenderId = user1Id
        });

        // User 1 should receive the message.
        Assert.AreEqual("Hello, world! 2", repo1.Head()?.Item.Content);

        // Kick user 2.
        await RunUnderUser("wsuser1", async () => { await Sdk.KickMemberAsync(thread1Id, user2Id.ToString()); });

        // User 1 sends a message. User 2 should not receive it.
        await repo1.Send(new ChatMessage
        {
            Content = "Hello, world! After kick!",
            Preview = "Hello, world! After kick!",
            SenderId = user1Id
        });
        Assert.AreEqual("Hello, world! 2", repo2.Head()?.Item.Content);

        // User 2 sends a message. User 1 should not receive it.
        await repo2.Send(new ChatMessage
        {
            Content = "Hello, world! 2 After kick!",
            Preview = "Hello, world! 2 After kick!",
            SenderId = user1Id
        });
        Assert.AreEqual("Hello, world! After kick!", repo1.Head()?.Item.Content);

        // Clean
        await repo1.Disconnect();
        await repo2.Disconnect();
    }

    [TestMethod]
    public async Task TestClearUserUnreadAmount()
    {
        var thread1Id = 0;
        var user1Ws = string.Empty;
        var user2Ws = string.Empty;
        var user1Id = Guid.Empty;

        await RunUnderUser("wsuser1", async () =>
        {
            var result1 = await Sdk.CreateFromScratchAsync(
                name: "Sample thread 1",
                allowSearchByName: true,
                allowMemberSoftInvitation: true,
                allowMembersSendMessages: true,
                allowDirectJoinWithoutInvitation: true,
                allowMembersEnlistAllMembers: true);
            thread1Id = result1.NewThreadId;
            user1Ws = (await Sdk.InitThreadWebSocketAsync(thread1Id)).WebSocketEndpoint;
            user1Id = Guid.Parse((await Sdk.MeAsync()).User.Id);
        });
        await RunUnderUser("wsuser2", async () =>
        {
            await Sdk.DirectJoinAsync(thread1Id);
            user2Ws = (await Sdk.InitThreadWebSocketAsync(thread1Id)).WebSocketEndpoint;
        });
        await RunUnderUser("wsuser3", async () => { await Sdk.DirectJoinAsync(thread1Id); });

        // User 1 connects while user 2 idle.
        var repo1 = await new KahlaMessagesRepo(user1Ws).ConnectAndMonitor();

        // User 1 sends a message.
        await repo1.Send(new ChatMessage
        {
            Content = "Hello, world!",
            Preview = "Hello, world!",
            SenderId = user1Id
        });
        await repo1.Send(new ChatMessage
        {
            Content = "Hello, world! 2",
            Preview = "Hello, world! 2",
            SenderId = user1Id
        });

        // User 2 should notice there are 2 unread messages.
        await RunUnderUser("wsuser2", async () =>
        {
            var threadDetails = await Sdk.ThreadDetailsJoinedAsync(thread1Id);
            Assert.AreEqual((uint)2, threadDetails.Thread.MessageContext.UnReadAmount);
        });

        // User 2 read the messages.
        var repo2 = await new KahlaMessagesRepo(user2Ws).ConnectAndMonitor();

        // User 2 should notice there are no unread messages.
        await RunUnderUser("wsuser2", async () =>
        {
            var threadDetails = await Sdk.ThreadDetailsJoinedAsync(thread1Id);
            Assert.AreEqual((uint)0, threadDetails.Thread.MessageContext.UnReadAmount);
        });

        // User 1 & 2 keeps sending messages, and user 1 & 2 should not notice any unread messages.
        for (int i = 0; i < 10; i++)
        {
            await repo2.Send(new ChatMessage
            {
                Content = "Hello, world from 2! " + i,
                Preview = "Hello, world from 2! " + i,
                SenderId = user1Id
            });
            await repo1.Send(new ChatMessage
            {
                Content = "Hello, world from 1! " + i,
                Preview = "Hello, world from 1! " + i,
                SenderId = user1Id
            });
        }

        await RunUnderUser("wsuser1", async () =>
        {
            var threadDetails = await Sdk.ThreadDetailsJoinedAsync(thread1Id);
            Assert.AreEqual((uint)0, threadDetails.Thread.MessageContext.UnReadAmount);
            Assert.AreEqual((uint)22, threadDetails.Thread.TotalMessages);
        });
        await RunUnderUser("wsuser2", async () =>
        {
            var threadDetails = await Sdk.ThreadDetailsJoinedAsync(thread1Id);
            Assert.AreEqual((uint)0, threadDetails.Thread.MessageContext.UnReadAmount);
            Assert.AreEqual((uint)22, threadDetails.Thread.TotalMessages);
        });
        await RunUnderUser("wsuser3", async () =>
        {
            var threadDetails = await Sdk.ThreadDetailsJoinedAsync(thread1Id);
            // User 3 totally has 22 unread messages.
            Assert.AreEqual((uint)22, threadDetails.Thread.MessageContext.UnReadAmount);
            Assert.AreEqual((uint)22, threadDetails.Thread.TotalMessages);
        });

        // Clean up.
        await repo1.Disconnect();
        await repo2.Disconnect();
    }

    [TestMethod]
    public async Task TestAfterRebootingUnreadAmountCorrect()
    {
        var thread1Id = 0;
        var user1Ws = string.Empty;
        var user2Ws = string.Empty;
        var user1Id = Guid.Empty;

        await RunUnderUser("wsuser1", async () =>
        {
            var result1 = await Sdk.CreateFromScratchAsync(
                name: "Sample thread 1",
                allowSearchByName: true,
                allowMemberSoftInvitation: true,
                allowMembersSendMessages: true,
                allowDirectJoinWithoutInvitation: true,
                allowMembersEnlistAllMembers: true);
            thread1Id = result1.NewThreadId;
            user1Ws = (await Sdk.InitThreadWebSocketAsync(thread1Id)).WebSocketEndpoint;
            user1Id = Guid.Parse((await Sdk.MeAsync()).User.Id);
        });
        await RunUnderUser("wsuser2", async () =>
        {
            await Sdk.DirectJoinAsync(thread1Id);
            user2Ws = (await Sdk.InitThreadWebSocketAsync(thread1Id)).WebSocketEndpoint;
        });
        await RunUnderUser("wsuser3", async () => { await Sdk.DirectJoinAsync(thread1Id); });

        // User 1 connects while user 2 idle.
        var repo1 = await new KahlaMessagesRepo(user1Ws).ConnectAndMonitor();

        // User 1 sends a message.
        await repo1.Send(new ChatMessage
        {
            Content = "Hello, world!",
            Preview = "Hello, world!",
            SenderId = user1Id
        });
        await repo1.Send(new ChatMessage
        {
            Content = "Hello, world! 2",
            Preview = "Hello, world! 2",
            SenderId = user1Id
        });
        await Task.Delay(1000);

        // User 2 should notice there are 2 unread messages.
        await RunUnderUser("wsuser2", async () =>
        {
            var threadDetails = await Sdk.ThreadDetailsJoinedAsync(thread1Id);
            Assert.AreEqual((uint)2, threadDetails.Thread.MessageContext.UnReadAmount);
        });

        // User 2 read the messages.
        var repo2 = await new KahlaMessagesRepo(user2Ws).ConnectAndMonitor();
        await Task.Delay(200); // Wait for the messages to be reflected.

        // User 2 should notice there are no unread messages.
        await RunUnderUser("wsuser2", async () =>
        {
            var threadDetails = await Sdk.ThreadDetailsJoinedAsync(thread1Id);
            Assert.AreEqual((uint)0, threadDetails.Thread.MessageContext.UnReadAmount);
        });

        // User 3 should notice there are 2 unread messages.
        await RunUnderUser("wsuser3", async () =>
        {
            var threadDetails = await Sdk.ThreadDetailsJoinedAsync(thread1Id);
            Assert.AreEqual((uint)2, threadDetails.Thread.MessageContext.UnReadAmount);
        });

        // Server reboot.
        await Server!.Services.GetRequiredService<QuickMessageAccess>().PersistUserUnreadAmount();
        await Server!.Services.GetRequiredService<QuickMessageAccess>().LoadAsync();

        // User 1 should notice there are no unread messages.
        await RunUnderUser("wsuser1", async () =>
        {
            var threadDetails = await Sdk.ThreadDetailsJoinedAsync(thread1Id);
            Assert.AreEqual((uint)0, threadDetails.Thread.MessageContext.UnReadAmount);
        });
        // User 2 should notice there are no unread messages.
        await RunUnderUser("wsuser2", async () =>
        {
            var threadDetails = await Sdk.ThreadDetailsJoinedAsync(thread1Id);
            Assert.AreEqual((uint)0, threadDetails.Thread.MessageContext.UnReadAmount);
        });

        // User 3 should notice there are 2 unread messages.
        await RunUnderUser("wsuser3", async () =>
        {
            var threadDetails = await Sdk.ThreadDetailsJoinedAsync(thread1Id);
            Assert.AreEqual((uint)2, threadDetails.Thread.MessageContext.UnReadAmount);
        });

        // Clean up.
        await repo1.Disconnect();
        await repo2.Disconnect();
    }

    [TestMethod]
    public async Task TestLargeTextReflection()
    {
        var thread1Id = 0;
        var user1Ws = string.Empty;
        var user2Ws = string.Empty;
        var user1Id = Guid.Empty;

        await RunUnderUser("wsuser1", async () =>
        {
            var result1 = await Sdk.CreateFromScratchAsync(
                name: "Sample thread 1",
                allowSearchByName: true,
                allowMemberSoftInvitation: true,
                allowMembersSendMessages: true,
                allowDirectJoinWithoutInvitation: true,
                allowMembersEnlistAllMembers: true);
            thread1Id = result1.NewThreadId;
            user1Ws = (await Sdk.InitThreadWebSocketAsync(thread1Id)).WebSocketEndpoint;
            user1Id = Guid.Parse((await Sdk.MeAsync()).User.Id);
        });
        await RunUnderUser("wsuser2", async () =>
        {
            await Sdk.DirectJoinAsync(thread1Id);
            user2Ws = (await Sdk.InitThreadWebSocketAsync(thread1Id)).WebSocketEndpoint;
        });

        var repo1 = await new KahlaMessagesRepo(user1Ws).ConnectAndMonitor();
        var repo2 = await new KahlaMessagesRepo(user2Ws).ConnectAndMonitor();

        // User 1 sends a message. Reflect to user 2.
        var largeString0xFFFF = new string('a', 0xFF00);
        await repo1.Send(new ChatMessage
        {
            Content = largeString0xFFFF,
            Preview = "Preview",
            SenderId = user1Id
        });

        // User 2 should receive the message.
        Assert.AreEqual(largeString0xFFFF, repo2.Head()?.Item.Content);
        Assert.AreEqual("Preview", repo2.Head()?.Item.Preview);

        // Clean
        await repo1.Disconnect();
        await repo2.Disconnect();
    }

    [TestMethod]
    public async Task TestAtUser()
    {
        var thread1Id = 0;
        var user1Ws = string.Empty;
        var user2Ws = string.Empty;
        var user1Id = Guid.Empty;
        var user2Id = Guid.Empty;
        
        await RunUnderUser("wsuser1", async () =>
        {
            var result1 = await Sdk.CreateFromScratchAsync(
                name: "Sample thread 1",
                allowSearchByName: true,
                allowMemberSoftInvitation: true,
                allowMembersSendMessages: true,
                allowDirectJoinWithoutInvitation: true,
                allowMembersEnlistAllMembers: true);
            thread1Id = result1.NewThreadId;
            user1Ws = (await Sdk.InitThreadWebSocketAsync(thread1Id)).WebSocketEndpoint;
            user1Id = Guid.Parse((await Sdk.MeAsync()).User.Id);
        });
        await RunUnderUser("wsuser2", async () =>
        {
            await Sdk.DirectJoinAsync(thread1Id);
            user2Ws = (await Sdk.InitThreadWebSocketAsync(thread1Id)).WebSocketEndpoint;
            user2Id = Guid.Parse((await Sdk.MeAsync()).User.Id);
        });
        
        // User 1 at user 2, user 2 shows he was ated. After reading, no unread at anymore.
        var repo1 = await new KahlaMessagesRepo(user1Ws).ConnectAndMonitor();
        await repo1.Send(new ChatMessage
        {
            Content = "Hello, world!",
            Preview = "Hello, world!",
            SenderId = user1Id,
            Ats = [user2Id]
        });
        
        await RunUnderUser("wsuser2", async () =>
        {
            var myThreads = await Sdk.MyThreadsAsync();
            Assert.AreEqual(1, myThreads.KnownThreads.Count);
            Assert.AreEqual((uint)1, myThreads.KnownThreads[0].MessageContext.UnReadAmount);
            Assert.AreEqual(true, myThreads.KnownThreads[0].MessageContext.LatestMessage!.Ats.Contains(user2Id));
            Assert.AreEqual(true, myThreads.KnownThreads[0].UnreadAtMe);
        });
        
        var repo2 = await new KahlaMessagesRepo(user2Ws).ConnectAndMonitor();
        await Task.Delay(200); // Wait for the messages to be reflected.
        
        await RunUnderUser("wsuser2", async () =>
        {
            var myThreads = await Sdk.MyThreadsAsync();
            Assert.AreEqual(1, myThreads.KnownThreads.Count);
            Assert.AreEqual((uint)0, myThreads.KnownThreads[0].MessageContext.UnReadAmount);
            Assert.AreEqual(true, myThreads.KnownThreads[0].MessageContext.LatestMessage!.Ats.Contains(user2Id));
            Assert.AreEqual(false, myThreads.KnownThreads[0].UnreadAtMe);
        });
        
        // Clean
        await repo1.Disconnect();
        await repo2.Disconnect();
    }
    
    [TestMethod]
    public async Task TestAtUserDirectSend()
    {
        var thread1Id = 0;
        var user2Ws = string.Empty;
        var user2Id = Guid.Empty;
        
        await RunUnderUser("wsuser1", async () =>
        {
            var result1 = await Sdk.CreateFromScratchAsync(
                name: "Sample thread 1",
                allowSearchByName: true,
                allowMemberSoftInvitation: true,
                allowMembersSendMessages: true,
                allowDirectJoinWithoutInvitation: true,
                allowMembersEnlistAllMembers: true);
            thread1Id = result1.NewThreadId;
        });
        await RunUnderUser("wsuser2", async () =>
        {
            await Sdk.DirectJoinAsync(thread1Id);
            user2Ws = (await Sdk.InitThreadWebSocketAsync(thread1Id)).WebSocketEndpoint;
            user2Id = Guid.Parse((await Sdk.MeAsync()).User.Id);
        });
        
        // User 1 at user 2, user 2 shows he was ated. After reading, no unread at anymore.
        await RunUnderUser("wsuser1", async () =>
        {
            await Sdk.DirectSendMessageAsync(
                threadId: thread1Id,
                content: "Hello, world!",
                preview: "Hello, world!",
                ats: [user2Id]);
        });
        
        await RunUnderUser("wsuser2", async () =>
        {
            var myThreads = await Sdk.MyThreadsAsync();
            Assert.AreEqual(1, myThreads.KnownThreads.Count);
            Assert.AreEqual((uint)1, myThreads.KnownThreads[0].MessageContext.UnReadAmount);
            Assert.AreEqual(true, myThreads.KnownThreads[0].MessageContext.LatestMessage!.Ats.Contains(user2Id));
            Assert.AreEqual(true, myThreads.KnownThreads[0].UnreadAtMe);
        });
        
        var repo2 = await new KahlaMessagesRepo(user2Ws).ConnectAndMonitor();
        await Task.Delay(200); // Wait for the messages to be reflected.
        
        await RunUnderUser("wsuser2", async () =>
        {
            var myThreads = await Sdk.MyThreadsAsync();
            Assert.AreEqual(1, myThreads.KnownThreads.Count);
            Assert.AreEqual((uint)0, myThreads.KnownThreads[0].MessageContext.UnReadAmount);
            Assert.AreEqual(true, myThreads.KnownThreads[0].MessageContext.LatestMessage!.Ats.Contains(user2Id));
            Assert.AreEqual(false, myThreads.KnownThreads[0].UnreadAtMe);
        });
        
        // Clean
        await repo2.Disconnect();
    }
    
    [TestMethod]
    public async Task TestAtUserWithReload()
    {
        var thread1Id = 0;
        var user1Ws = string.Empty;
        var user2Ws = string.Empty;
        var user1Id = Guid.Empty;
        var user2Id = Guid.Empty;
        
        await RunUnderUser("wsuser1", async () =>
        {
            var result1 = await Sdk.CreateFromScratchAsync(
                name: "Sample thread 1",
                allowSearchByName: true,
                allowMemberSoftInvitation: true,
                allowMembersSendMessages: true,
                allowDirectJoinWithoutInvitation: true,
                allowMembersEnlistAllMembers: true);
            thread1Id = result1.NewThreadId;
            user1Ws = (await Sdk.InitThreadWebSocketAsync(thread1Id)).WebSocketEndpoint;
            user1Id = Guid.Parse((await Sdk.MeAsync()).User.Id);
        });
        await RunUnderUser("wsuser2", async () =>
        {
            await Sdk.DirectJoinAsync(thread1Id);
            user2Ws = (await Sdk.InitThreadWebSocketAsync(thread1Id)).WebSocketEndpoint;
            user2Id = Guid.Parse((await Sdk.MeAsync()).User.Id);
        });
        
        // User 1 at user 2, user 2 shows he was ated. After reading, no unread at anymore.
        var repo1 = await new KahlaMessagesRepo(user1Ws).ConnectAndMonitor();
        await repo1.Send(new ChatMessage
        {
            Content = "Hello, world!",
            Preview = "Hello, world!",
            SenderId = user1Id,
            Ats = [user2Id]
        });
        
        // Reload server.
        await Server!.Services.GetRequiredService<QuickMessageAccess>().PersistUserUnreadAmount();
        await Server!.Services.GetRequiredService<QuickMessageAccess>().LoadAsync();
        
        await RunUnderUser("wsuser2", async () =>
        {
            var myThreads = await Sdk.MyThreadsAsync();
            Assert.AreEqual(1, myThreads.KnownThreads.Count);
            Assert.AreEqual((uint)1, myThreads.KnownThreads[0].MessageContext.UnReadAmount);
            Assert.AreEqual(true, myThreads.KnownThreads[0].MessageContext.LatestMessage!.Ats.Contains(user2Id));
            Assert.AreEqual(true, myThreads.KnownThreads[0].UnreadAtMe);
        });
        
        var repo2 = await new KahlaMessagesRepo(user2Ws).ConnectAndMonitor();
        await Task.Delay(200); // Wait for the messages to be reflected.
        
        await RunUnderUser("wsuser2", async () =>
        {
            var myThreads = await Sdk.MyThreadsAsync();
            Assert.AreEqual(1, myThreads.KnownThreads.Count);
            Assert.AreEqual((uint)0, myThreads.KnownThreads[0].MessageContext.UnReadAmount);
            Assert.AreEqual(true, myThreads.KnownThreads[0].MessageContext.LatestMessage!.Ats.Contains(user2Id));
            Assert.AreEqual(false, myThreads.KnownThreads[0].UnreadAtMe);
        });
        
        // Clean
        await repo1.Disconnect();
        await repo2.Disconnect();
    }
    
    [TestMethod]
    public async Task TestAtUserWebPushedInfo()
    {
        var thread1Id = 0;
        var user1Ws = string.Empty;
        var user1Id = Guid.Empty;
        var user2Id = Guid.Empty;
        
        await RunUnderUser("wsuser1", async () =>
        {
            var result1 = await Sdk.CreateFromScratchAsync(
                name: "Sample thread 1",
                allowSearchByName: true,
                allowMemberSoftInvitation: true,
                allowMembersSendMessages: true,
                allowDirectJoinWithoutInvitation: true,
                allowMembersEnlistAllMembers: true);
            thread1Id = result1.NewThreadId;
            user1Ws = (await Sdk.InitThreadWebSocketAsync(thread1Id)).WebSocketEndpoint;
            user1Id = Guid.Parse((await Sdk.MeAsync()).User.Id);
            
            // This device won't receive because it's the sender's device.
            await Sdk.AddDeviceAsync("fake name", "fake push auth", "fake endpoint", "fake p256dh");
        });
        await RunUnderUser("wsuser2", async () =>
        {
            await Sdk.DirectJoinAsync(thread1Id);
            user2Id = Guid.Parse((await Sdk.MeAsync()).User.Id);

            await Sdk.AddDeviceAsync("fake name", "fake push auth", "fake endpoint", "fake p256dh");
        });

        // User 1 at user 2, user 2 shows he was ated. After reading, no unread at anymore.
        var repo1 = await new KahlaMessagesRepo(user1Ws).ConnectAndMonitor();

        // User 1 will send a message in 1 second
        _ = Task.Run(async () =>
        {
            await Task.Delay(1000);
            await repo1.Send(new ChatMessage
            {
                Content = "Hello, world!",
                Preview = "Hello, world!",
                SenderId = user1Id,
                Ats = [user2Id]
            });
            
            await repo1.Send(new ChatMessage
            {
                Content = "Hello, world no at!",
                Preview = "Hello, world no at!",
                SenderId = user1Id,
            });
        });
        
        // User 103 will receive a NewMessageEvent
        await RunUnderUser("wsuser2", async () =>
        {
            var pushed = await RunAndGetEvent(async () =>
            {
                await Task.CompletedTask;
            });

            Assert.IsTrue(pushed is NewMessageEvent);
            Assert.AreEqual("Hello, world!", (pushed as NewMessageEvent)!.Message.Preview);
            Assert.AreEqual(user2Id, (pushed as NewMessageEvent)!.Message.Ats[0]);
            Assert.AreEqual(true, (pushed as NewMessageEvent)!.Mentioned);
        }, autoSignOut: false);

        await Task.Delay(200);
        Assert.AreEqual(2, MockWebPushService.PushedPayloads.Count);
        Assert.IsTrue(MockWebPushService.PushedPayloads[0] is NewMessageEvent);
        Assert.AreEqual("Hello, world!", (MockWebPushService.PushedPayloads[0] as NewMessageEvent)!.Message.Preview);
        Assert.AreEqual(true, (MockWebPushService.PushedPayloads[0] as NewMessageEvent)!.Mentioned);
        Assert.AreEqual("Sample thread 1", (MockWebPushService.PushedPayloads[0] as NewMessageEvent)!.ThreadName);
        Assert.IsTrue(MockWebPushService.PushedPayloads[1] is NewMessageEvent);
        Assert.AreEqual("Hello, world no at!", (MockWebPushService.PushedPayloads[1] as NewMessageEvent)!.Message.Preview);
        Assert.AreEqual(false, (MockWebPushService.PushedPayloads[1] as NewMessageEvent)!.Mentioned);
        Assert.AreEqual("Sample thread 1", (MockWebPushService.PushedPayloads[1] as NewMessageEvent)!.ThreadName);

        // Clean
        await repo1.Disconnect();
    }

    [TestMethod]
    public async Task TestNoWebPushIfUserMuted()
    {
        var thread1Id = 0;
        var user1Ws = string.Empty;
        var user1Id = Guid.Empty;
        
        await RunUnderUser("wsuser1", async () =>
        {
            var result1 = await Sdk.CreateFromScratchAsync(
                name: "Sample thread 1",
                allowSearchByName: true,
                allowMemberSoftInvitation: true,
                allowMembersSendMessages: true,
                allowDirectJoinWithoutInvitation: true,
                allowMembersEnlistAllMembers: true);
            thread1Id = result1.NewThreadId;
            user1Ws = (await Sdk.InitThreadWebSocketAsync(thread1Id)).WebSocketEndpoint;
            user1Id = Guid.Parse((await Sdk.MeAsync()).User.Id);
            
            // This device won't receive because it's the sender's device.
            await Sdk.AddDeviceAsync("fake name", "fake push auth", "fake endpoint", "fake p256dh");
        });
        await RunUnderUser("wsuser2", async () =>
        {
            await Sdk.DirectJoinAsync(thread1Id);
            await Sdk.AddDeviceAsync("fake name", "fake push auth", "fake endpoint", "fake p256dh");
            await Sdk.SetMuteAsync(thread1Id, true);
        });

        // User 1 at user 2, user 2 shows he was ated. After reading, no unread at anymore.
        var repo1 = await new KahlaMessagesRepo(user1Ws).ConnectAndMonitor();
        
        // User 1 will send a message in 1 second
        _ = Task.Run(async () =>
        {
            await Task.Delay(1000);
            await repo1.Send(new ChatMessage
            {
                Content = "Hello, world!",
                Preview = "Hello, world!",
                SenderId = user1Id,
                Ats = []
            });
        });
        
        // User 103 will receive a NewMessageEvent
        await RunUnderUser("wsuser2", async () =>
        {
            var pushed = await RunAndGetEvent(async () =>
            {
                await Task.CompletedTask;
            });

            Assert.IsTrue(pushed is NewMessageEvent);
            Assert.AreEqual("Hello, world!", (pushed as NewMessageEvent)!.Message.Preview);
            Assert.AreEqual(0, (pushed as NewMessageEvent)!.Message.Ats.Length);
            Assert.AreEqual(false, (pushed as NewMessageEvent)!.Mentioned);
        }, autoSignOut: false);

        await Task.Delay(200);
        Assert.AreEqual(0, MockWebPushService.PushedPayloads.Count);

        // Clean
        await repo1.Disconnect();
    }

    [TestMethod]
    public async Task TestWebPushIfUserMutedByAtHim()
    {
        var thread1Id = 0;
        var user1Ws = string.Empty;
        var user1Id = Guid.Empty;
        var user2Id = Guid.Empty;
        
        await RunUnderUser("wsuser1", async () =>
        {
            var result1 = await Sdk.CreateFromScratchAsync(
                name: "Sample thread 1",
                allowSearchByName: true,
                allowMemberSoftInvitation: true,
                allowMembersSendMessages: true,
                allowDirectJoinWithoutInvitation: true,
                allowMembersEnlistAllMembers: true);
            thread1Id = result1.NewThreadId;
            user1Ws = (await Sdk.InitThreadWebSocketAsync(thread1Id)).WebSocketEndpoint;
            user1Id = Guid.Parse((await Sdk.MeAsync()).User.Id);
            
            // This device won't receive because it's the sender's device.
            await Sdk.AddDeviceAsync("fake name", "fake push auth", "fake endpoint", "fake p256dh");
        });
        await RunUnderUser("wsuser2", async () =>
        {
            user2Id = Guid.Parse((await Sdk.MeAsync()).User.Id);
            await Sdk.DirectJoinAsync(thread1Id);
            await Sdk.AddDeviceAsync("fake name", "fake push auth", "fake endpoint", "fake p256dh");
            await Sdk.SetMuteAsync(thread1Id, true);
        });

        // User 1 at user 2, user 2 shows he was ated. After reading, no unread at anymore.
        var repo1 = await new KahlaMessagesRepo(user1Ws).ConnectAndMonitor();
        
        // User 1 will send a message in 1 second
        _ = Task.Run(async () =>
        {
            await Task.Delay(1000);
            await repo1.Send(new ChatMessage
            {
                Content = "Hello, world!",
                Preview = "Hello, world!",
                SenderId = user1Id,
                Ats = [user2Id]
            });
        });
        
        // User 103 will receive a NewMessageEvent
        await RunUnderUser("wsuser2", async () =>
        {
            var pushed = await RunAndGetEvent(async () =>
            {
                await Task.CompletedTask;
            });

            Assert.IsTrue(pushed is NewMessageEvent);
            Assert.AreEqual("Hello, world!", (pushed as NewMessageEvent)!.Message.Preview);
            Assert.AreEqual(1, (pushed as NewMessageEvent)!.Message.Ats.Length);
            Assert.AreEqual(true, (pushed as NewMessageEvent)!.Mentioned);
        }, autoSignOut: false);

        await Task.Delay(200);
        Assert.AreEqual(1, MockWebPushService.PushedPayloads.Count);
        Assert.IsTrue(MockWebPushService.PushedPayloads[0] is NewMessageEvent);
        Assert.AreEqual("Hello, world!", (MockWebPushService.PushedPayloads[0] as NewMessageEvent)!.Message.Preview);
        Assert.AreEqual(true, (MockWebPushService.PushedPayloads[0] as NewMessageEvent)!.Mentioned);
        Assert.AreEqual("Sample thread 1", (MockWebPushService.PushedPayloads[0] as NewMessageEvent)!.ThreadName);
        Assert.AreEqual("wsuser1", (MockWebPushService.PushedPayloads[0] as NewMessageEvent)!.Message.Sender!.NickName);

        // Clean
        await repo1.Disconnect();
    }
}