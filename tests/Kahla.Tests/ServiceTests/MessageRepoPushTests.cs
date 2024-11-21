using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aiursoft.Kahla.Tests.ServiceTests;

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