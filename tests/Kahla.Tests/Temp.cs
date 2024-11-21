using Aiursoft.AiurEventSyncer.Abstract;
using Aiursoft.AiurObserver;
using Aiursoft.AiurObserver.WebSocket;
using Aiursoft.Kahla.SDK.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Extensions = Aiursoft.Kahla.Server.Extensions;

namespace Aiursoft.Kahla.Tests;

// Refactor the message repo. It should be able to be more efficient.
public class KahlaMessagesRepo(string webSocketEndpoint)
{
    private ObservableWebSocket? _webSocket;
    private Task _listenTask = Task.CompletedTask;
    private readonly KahlaMessagesMemoryStore _messages = new();

    public async Task<KahlaMessagesRepo> ConnectAndMonitor(bool inCurrentThread = false)
    {
        var realEndpoint = webSocketEndpoint + "?start=" + _messages.PulledItemsOffset;
        _webSocket = await realEndpoint.ConnectAsWebSocketServer();
        await Push();
        _webSocket.Subscribe(OnNewWebSocketMessage);
        var listenTask = _webSocket.Listen();
        if (inCurrentThread)
        {
            await listenTask;
        }
        else
        {
            _listenTask = Task.Run(async () => await listenTask);
        }
        return this;
    }
    
    public async Task Send(ChatMessage message, bool waitShortTime = true)
    {
        _messages.Commit(message);
        await Push();
        if (waitShortTime)
        {
            await Task.Delay(100);
        }
    }
    
    public void CommitOnly(ChatMessage message)
    {
        _messages.Commit(message);
    }
    
    public IEnumerable<Commit<ChatMessage>> GetAllMessages()
    {
        return _messages.GetAllMessagesEnumerable();
    }

    public Commit<ChatMessage>? Head()
    {
        return _messages.GetHead();
    }

    public async Task WaitTilListenTaskComplete()
    {
        await _listenTask;
    }

    public async Task Disconnect()
    {
        if (_webSocket != null)
        {
            await _webSocket.Close();
        }

        _webSocket = null;
    }

    private Task OnNewWebSocketMessage(string content)
    {
        var commits = Extensions.Deserialize<Commit<ChatMessage>[]>(content);
        foreach (var commit in commits)
        {
            _messages.OnPulledMessage(commit);
        }
        return Task.CompletedTask;
    }

    private async Task Push()
    {
        if (_webSocket == null || !_webSocket.Connected)
        {
            throw new InvalidOperationException("WebSocket is not connected!");
        }

        var messages = _messages.Push().ToArray();
        if (messages.Any())
        {
            var content = Extensions.Serialize(messages);
            await _webSocket.Send(content);
        }
    }
}

public class KahlaMessagesMemoryStore
{
    private readonly LinkedList<Commit<ChatMessage>> _messages = new();

    public LinkedListNode<Commit<ChatMessage>>? LastPulled;
    public int PulledItemsOffset;

    public LinkedListNode<Commit<ChatMessage>>? LastPushed;
    public int PushedItemsOffset;

    public void Commit(ChatMessage message)
    {
        var commit = new Commit<ChatMessage>
        {
            Id = Guid.NewGuid().ToString(),
            Item = message
        };
        _messages.AddLast(commit);
    }

    public void Commit(Commit<ChatMessage> commit)
    {
        _messages.AddLast(commit);
    }

    public void OnPulledMessage(Commit<ChatMessage> commit)
    {
        bool itemInserted = false;

        if (_messages.First == null)
        {
            // Message store is empty, add the commit as the first message.
            _messages.AddFirst(commit);
            LastPulled = _messages.First;
            PulledItemsOffset++;
        }
        else
        {
            var nextNode = GetNextNodeForPull();
            if (nextNode != null && nextNode.Value.Id == commit.Id)
            {
                // The pulled commit matches the next message; advance LastPulled.
                LastPulled = nextNode;
                PulledItemsOffset++;
            }
            else
            {
                // Insert the pulled commit before the next node.
                if (nextNode != null)
                {
                    _messages.AddBefore(nextNode, commit);
                    LastPulled = nextNode.Previous;
                }
                else
                {
                    // Next node is null, so add at the end.
                    _messages.AddLast(commit);
                    LastPulled = _messages.Last;
                }
                PulledItemsOffset++;
                itemInserted = true;
            }
        }

        UpdateLastPushed(itemInserted);
    }
    
    private void UpdateLastPushed(bool itemInserted)
    {
        if (LastPulled?.Previous == LastPushed)
        {
            // The LastPulled node is immediately after LastPushed; advance LastPushed.
            LastPushed = LastPulled;
            PushedItemsOffset++;
        }
        else if (itemInserted)
        {
            // A new item was inserted before LastPushed; adjust PushedItemsOffset.
            PushedItemsOffset++;
        }
    }

    private LinkedListNode<Commit<ChatMessage>>? GetNextNodeForPull()
    {
        if (LastPulled == null)
        {
            // No messages have been pulled yet; start from the first message.
            return _messages.First;
        }
        else
        {
            // Start from the node after LastPulled.
            return LastPulled.Next;
        }
    }


    public IEnumerable<Commit<ChatMessage>> Push()
    {
        if (_messages.First == null)
        {
            yield break;
        }
        var pushBegin = LastPushed == null ? _messages.First : LastPushed.Next;
        while (pushBegin != null)
        {
            LastPushed = pushBegin;
            yield return LastPushed.Value;
            pushBegin = pushBegin.Next;
            PushedItemsOffset++;
        }
    }

    public Commit<ChatMessage>[] GetAllMessages()
    {
        return _messages.ToArray();
    }
    
    public IEnumerable<Commit<ChatMessage>> GetAllMessagesEnumerable()
    {
        return _messages;
    }
    
    public Commit<ChatMessage>? GetHead()
    {
        return _messages.Last?.Value;
    }
}

[TestClass]
public class MessageRepoPushTests
{
    [TestMethod]
    public void TestEmptyPush()
    {
        var messagesStore = new KahlaMessagesMemoryStore();
        var emptyPush = messagesStore.Push().ToArray();
        Assert.AreEqual(0, emptyPush.Length);

        var emptyPushAgain = messagesStore.Push().ToArray();
        Assert.AreEqual(0, emptyPushAgain.Length);
    }

    [TestMethod]
    public void TestPushCommits()
    {
        var messagesStore = new KahlaMessagesMemoryStore();
        var message = new ChatMessage
        {
            Content = "Hello, world!"
        };
        messagesStore.Commit(message);

        var pushed = messagesStore.Push().ToArray();
        Assert.AreEqual(1, pushed.Length);
    }

    [TestMethod]
    public void TestPush3CommitsThen2Commits()
    {
        var messagesStore = new KahlaMessagesMemoryStore();
        messagesStore.Commit(new ChatMessage
        {
            Content = "message 1"
        });
        messagesStore.Commit(new ChatMessage
        {
            Content = "message 2"
        });
        messagesStore.Commit(new ChatMessage
        {
            Content = "message 3"
        });
        var initialPush = messagesStore.Push().ToArray();
        Assert.AreEqual(3, initialPush.Length);
        for (int i = 0; i < 3; i++)
        {
            Assert.AreEqual($"message {i + 1}", initialPush[i].Item.Content);
        }

        messagesStore.Commit(new ChatMessage
        {
            Content = "message 4"
        });
        messagesStore.Commit(new ChatMessage
        {
            Content = "message 5"
        });
        var secondPush = messagesStore.Push().ToArray();
        Assert.AreEqual(2, secondPush.Length);
        for (int i = 0; i < 2; i++)
        {
            Assert.AreEqual($"message {i + 4}", secondPush[i].Item.Content);
        }
    }
}

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
        Assert.AreEqual(3, initialPush.Length);

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
        Assert.AreEqual(3, initialPush.Length);
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
        Assert.AreEqual(3, initialPush.Length);

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
        Assert.AreEqual(0, pushed.Length);
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
        Assert.AreEqual(3, allMessages.Length);
        Assert.AreEqual(3, messagesStore.PushedItemsOffset);
        Assert.AreEqual(3, messagesStore.PulledItemsOffset);

        var pushed = messagesStore.Push().ToArray();
        Assert.AreEqual(0, pushed.Length);

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
        
        // Push should do nothing.
        var pushed = messagesStore.Push().ToArray();
        Assert.AreEqual(0, pushed.Length);
    }
}