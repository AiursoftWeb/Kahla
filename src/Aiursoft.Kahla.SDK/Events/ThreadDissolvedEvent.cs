using Aiursoft.Kahla.SDK.Events.Abstractions;

namespace Aiursoft.Kahla.SDK.Events;

/// <summary>
/// A thread that you are in has been dissolved.
///
/// In this case, client should remove the thread from the thread list.
/// </summary>
public class ThreadDissolvedEvent : KahlaEvent
{
    public ThreadDissolvedEvent()
    {
        Type = EventType.ThreadDissolved;
    }

    public required int ThreadId { get; init; }
    
    public required string ThreadName { get; init; }
}