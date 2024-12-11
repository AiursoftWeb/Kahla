using Aiursoft.Kahla.SDK.Events.Abstractions;
using Aiursoft.Kahla.SDK.Models.Mapped;

namespace Aiursoft.Kahla.SDK.Events;

/// <summary>
/// You have been directly invited to a thread.
///
/// In this case, client should add the thread to the thread list.
/// </summary>
public class YouDirectJoinedEvent : KahlaEvent
{
    public YouDirectJoinedEvent()
    {
        Type = EventType.YouDirectJoined;
    }

    public required KahlaThreadMappedJoinedView Thread { get; init; }
}