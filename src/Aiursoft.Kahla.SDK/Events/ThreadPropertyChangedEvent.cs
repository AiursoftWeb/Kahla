using Aiursoft.Kahla.SDK.Events.Abstractions;

namespace Aiursoft.Kahla.SDK.Events;

public class ThreadPropertyChangedEvent : KahlaEvent
{
    public ThreadPropertyChangedEvent()
    {
        Type = EventType.ThreadPropertyChanged;
    }

    public required int ThreadId { get; init; }
    
    public required string ThreadName { get; init; }
    
    public required string ThreadImagePath { get; init; }
}