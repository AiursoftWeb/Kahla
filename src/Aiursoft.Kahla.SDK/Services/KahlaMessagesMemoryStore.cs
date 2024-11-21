using Aiursoft.Kahla.SDK.Models;

namespace Aiursoft.Kahla.SDK.Services;

public class Commit<T>
{
    public string Id { get; init; } = Guid.NewGuid().ToString("D");
    public required T Item { get; init; }
    public DateTime CommitTime { get; set; } = DateTime.UtcNow;

    public override string ToString()
    {
        return $"{Item}";
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
