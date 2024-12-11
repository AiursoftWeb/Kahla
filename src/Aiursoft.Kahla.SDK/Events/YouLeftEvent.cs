using Aiursoft.Kahla.SDK.Events.Abstractions;

namespace Aiursoft.Kahla.SDK.Events;

/// <summary>
/// You have left a thread.
///
/// In this case, client should remove the thread from the thread list.
/// </summary>
public class YouLeftEvent : KahlaEvent
{
    public YouLeftEvent()
    {
        Type = EventType.YouLeft;
    }

    public required int ThreadId { get; init; }
    
    public required string ThreadName { get; init; }
}