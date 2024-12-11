using Aiursoft.Kahla.SDK.Events.Abstractions;

namespace Aiursoft.Kahla.SDK.Events;

/// <summary>
/// You have been kicked from a thread.
///
/// In this case, client should remove the thread from the thread list.
/// </summary>
public class YouBeenKickedEvent : KahlaEvent
{
    public YouBeenKickedEvent()
    {
        Type = EventType.YouBeenKicked;
    }

    public required int ThreadId { get; init; }
    
    public required string ThreadName { get; init; }
}