using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Services;

namespace Aiursoft.Kahla.Tests.ServiceTests;

[TestClass]
public class MessageRepoPullTests
{
    [TestMethod]
    public void TestPullExistingMessages()
    {
        var messagesStore = new KahlaMessagesMemoryStore();

        // Commit local messages with IDs 1, 2, 3
        var localMessages = new List<ChatMessage>
        {
            new ChatMessage { Content = "Local message 1" },
            new ChatMessage { Content = "Local message 2" },
            new ChatMessage { Content = "Local message 3" }
        };

        foreach (var message in localMessages)
        {
            messagesStore.Commit(message);
        }

        // Push the local messages
        var initialPush = messagesStore.Push().ToArray();
        Assert.HasCount(3, initialPush);

        // Simulate pulling messages that already exist locally (IDs match)
        var pulledCommits = initialPush; // Simulating same commits being pulled
        foreach (var commit in pulledCommits)
        {
            messagesStore.OnPulledMessage(commit);
        }

        // Ensure that no duplicates are created and pointers are advanced correctly
        var allMessages = messagesStore.GetAllMessages();
        Assert.AreEqual(3, allMessages.Count());
        Assert.AreEqual("Local message 3", allMessages.Last().Item.Content);
        Assert.AreEqual("Local message 3", messagesStore.LastPulled?.Value.Item.Content);
    }

    [TestMethod]
    public void TestPullNewMessages()
    {
        var messagesStore = new KahlaMessagesMemoryStore();

        // Commit local messages with IDs 1, 2, 3
        var localMessages = new List<ChatMessage>
        {
            new() { Content = "Local message 1" },
            new() { Content = "Local message 2" },
            new() { Content = "Local message 3" }
        };

        foreach (var message in localMessages)
        {
            messagesStore.Commit(message);
        }

        // Push the local messages
        var initialPush = messagesStore.Push().ToArray();
        Assert.HasCount(3, initialPush);
        Assert.AreEqual(3, messagesStore.PushedItemsOffset);

        var newPulledCommits = new List<Commit<ChatMessage>>
        {
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Item = new ChatMessage { Content = "Pulled message 4" }
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Item = new ChatMessage { Content = "Pulled message 5" }
            }
        };

        foreach (var commit in newPulledCommits)
        {
            messagesStore.OnPulledMessage(commit);
        }

        var allMessages = messagesStore.GetAllMessages().ToArray();
        Assert.AreEqual(5, allMessages.Count());
        Assert.AreEqual("Pulled message 4", allMessages[0].Item.Content);
        Assert.AreEqual("Pulled message 5", allMessages[1].Item.Content);
        Assert.AreEqual("Local message 1", allMessages[2].Item.Content);
        Assert.AreEqual("Local message 2", allMessages[3].Item.Content);
        Assert.AreEqual("Local message 3", allMessages[4].Item.Content);
        
        Assert.AreEqual("Pulled message 5", messagesStore.LastPulled?.Value.Item.Content);
        Assert.AreEqual("Local message 3", messagesStore.LastPushed?.Value.Item.Content);
        Assert.AreEqual(2, messagesStore.PulledItemsOffset);
        Assert.AreEqual(5, messagesStore.PushedItemsOffset);
    }

    [TestMethod]
    public void TestCommitPushAndPullWillConsolidate()
    {
        var messagesStore = new KahlaMessagesMemoryStore();

        // Commit local messages with IDs 1, 2, 3
        var localMessages = new List<Commit<ChatMessage>>
        {
            new()
            {
                Id = Guid.NewGuid().ToString(), Item = new ChatMessage { Content = "Local message 1" }
            },
            new()
            {
                Id = Guid.NewGuid().ToString(), Item = new ChatMessage { Content = "Local message 2" }
            },
            new()
            {
                Id = Guid.NewGuid().ToString(), Item = new ChatMessage { Content = "Local message 3" }
            }
        };

        foreach (var message in localMessages)
        {
            messagesStore.Commit(message);
        }

        var initialPush = messagesStore.Push().ToArray();
        Assert.HasCount(3, initialPush);

        foreach (var commit in localMessages)
        {
            messagesStore.OnPulledMessage(commit);
        }

        var allMessages = messagesStore.GetAllMessages().ToArray();
        Assert.AreEqual(3, allMessages.Count());
        Assert.AreEqual(3, messagesStore.PushedItemsOffset);
        Assert.AreEqual(3, messagesStore.PulledItemsOffset);
    }

    [TestMethod]
    public void TestCommitPullAndPushGetNothing()
    {
        var messagesStore = new KahlaMessagesMemoryStore();

        // Commit local messages with IDs 1, 2, 3
        var localMessages = new List<Commit<ChatMessage>>
        {
            new() { Id = Guid.NewGuid().ToString(), Item = new ChatMessage { Content = "Local message 1" } },
            new() { Id = Guid.NewGuid().ToString(), Item = new ChatMessage { Content = "Local message 2" } },
            new() { Id = Guid.NewGuid().ToString(), Item = new ChatMessage { Content = "Local message 3" } }
        };

        foreach (var message in localMessages)
        {
            messagesStore.Commit(message);
        }

        foreach (var commit in localMessages)
        {
            messagesStore.OnPulledMessage(commit);
        }

        var allMessages = messagesStore.GetAllMessages().ToArray();
        Assert.AreEqual(3, allMessages.Count());
        Assert.AreEqual(3, messagesStore.PushedItemsOffset);
        Assert.AreEqual(3, messagesStore.PulledItemsOffset);

        var pushed = messagesStore.Push().ToArray();
        Assert.IsEmpty(pushed);
    }

    [TestMethod]
    public void TestPullTwice()
    {
        var messagesStore = new KahlaMessagesMemoryStore();

        // Commit local messages with IDs 1, 2, 3
        var localMessages = new List<Commit<ChatMessage>>
        {
            new() { Id = Guid.NewGuid().ToString(), Item = new ChatMessage { Content = "Local message 1" } },
            new() { Id = Guid.NewGuid().ToString(), Item = new ChatMessage { Content = "Local message 2" } },
            new() { Id = Guid.NewGuid().ToString(), Item = new ChatMessage { Content = "Local message 3" } }
        };

        foreach (var commit in localMessages)
        {
            messagesStore.OnPulledMessage(commit);
        }

        var allMessages = messagesStore.GetAllMessages().ToArray();
        Assert.HasCount(3, allMessages);
        Assert.AreEqual(3, messagesStore.PushedItemsOffset);
        Assert.AreEqual(3, messagesStore.PulledItemsOffset);

        var pushed = messagesStore.Push().ToArray();
        Assert.IsEmpty(pushed);

        var localMessages2 = new List<Commit<ChatMessage>>
        {
            new() { Id = Guid.NewGuid().ToString(), Item = new ChatMessage { Content = "Local message 4" } },
            new() { Id = Guid.NewGuid().ToString(), Item = new ChatMessage { Content = "Local message 5" } },
        };

        foreach (var commit in localMessages2)
        {
            messagesStore.OnPulledMessage(commit);
        }

        var allMessages2 = messagesStore.GetAllMessages().ToArray();
        Assert.AreEqual(5, allMessages2.Count());
        Assert.AreEqual(5, messagesStore.PushedItemsOffset);
        Assert.AreEqual(5, messagesStore.PulledItemsOffset);
    }

    [TestMethod]
    public void TestPullWhilePushing()
    {
        var messagesStore = new KahlaMessagesMemoryStore();

        // Commit local messages with IDs 1, 2, 3
        var localMessages = new List<Commit<ChatMessage>>
        {
            new() { Id = Guid.NewGuid().ToString(), Item = new ChatMessage { Content = "Local message 1" } },
            new() { Id = Guid.NewGuid().ToString(), Item = new ChatMessage { Content = "Local message 2" } },
            new() { Id = Guid.NewGuid().ToString(), Item = new ChatMessage { Content = "Local message 3" } }
        };
        
        foreach (var commit in localMessages)
        {
            messagesStore.Commit(commit);
        }
        foreach (var commit in messagesStore.Push())
        {
            messagesStore.OnPulledMessage(commit);
        }
        
        var allMessages = messagesStore.GetAllMessages().ToArray();
        Assert.AreEqual(3, allMessages.Count());
        Assert.AreEqual(3, messagesStore.PushedItemsOffset);
        Assert.AreEqual(3, messagesStore.PulledItemsOffset);
    }
    
    [TestMethod]
    public void TestMerge()
    {
        var messagesStore = new KahlaMessagesMemoryStore();

        // Commit local messages with IDs 1, 2, 3
        var localMessages = new List<Commit<ChatMessage>>
        {
            new() { Id = Guid.NewGuid().ToString(), Item = new ChatMessage { Content = "Local message 1" } },
            new() { Id = Guid.NewGuid().ToString(), Item = new ChatMessage { Content = "Local message 2" } },
            new() { Id = Guid.NewGuid().ToString(), Item = new ChatMessage { Content = "Local message 3" } }
        };
        
        foreach (var commit in localMessages)
        {
            messagesStore.Commit(commit);
        }
        foreach (var commit in messagesStore.Push())
        {
            messagesStore.OnPulledMessage(commit);
        }
        
        // Everything in sync.
        var allMessages = messagesStore.GetAllMessages().ToArray();
        Assert.AreEqual(3, allMessages.Count());
        Assert.AreEqual(3, messagesStore.PushedItemsOffset);
        Assert.AreEqual(3, messagesStore.PulledItemsOffset);
        
        // Commit 6,7,8
        // Pulled 4,5,6,7,8,9
        var localMessages2 = new List<Commit<ChatMessage>>
        {
            new() { Id = Guid.NewGuid().ToString(), Item = new ChatMessage { Content = "Local message 6" } },
            new() { Id = Guid.NewGuid().ToString(), Item = new ChatMessage { Content = "Local message 7" } },
            new() { Id = Guid.NewGuid().ToString(), Item = new ChatMessage { Content = "Local message 8" } }
        };
        foreach (var commit in localMessages2)
        {
            messagesStore.Commit(commit);
        }
        
        var toBePulled = new List<Commit<ChatMessage>>
        {
            new() { Id = Guid.NewGuid().ToString(), Item = new ChatMessage { Content = "Pulled message 4" } },
            new() { Id = Guid.NewGuid().ToString(), Item = new ChatMessage { Content = "Pulled message 5" } },
            localMessages2[0],
            localMessages2[1],
            localMessages2[2],
            new() { Id = Guid.NewGuid().ToString(), Item = new ChatMessage { Content = "Pulled message 9" } }
        };
        foreach (var commit in toBePulled)
        {
            messagesStore.OnPulledMessage(commit);
        }
        
        // Finally: 1,2,3,4,5,6,7,8,9
        // Pulled till 9. So pushed should be 9.
        allMessages = messagesStore.GetAllMessages().ToArray();
        Assert.AreEqual(9, allMessages.Count());
        Assert.AreEqual(9, messagesStore.PushedItemsOffset);
        Assert.AreEqual(9, messagesStore.PulledItemsOffset);
        
        for (int i = 1; i <= 9; i++)
        {
            Assert.EndsWith(i.ToString(), allMessages[i - 1].Item.Content);
        }
        
        // Push should do nothing.
        var pushed = messagesStore.Push().ToArray();
        Assert.IsEmpty(pushed);
    }
}