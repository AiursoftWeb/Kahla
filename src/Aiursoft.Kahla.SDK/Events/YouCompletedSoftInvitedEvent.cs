using Aiursoft.Kahla.SDK.Events.Abstractions;
using Aiursoft.Kahla.SDK.Models.Mapped;

namespace Aiursoft.Kahla.SDK.Events;

/// <summary>
/// You completed a software invite and soft joined a thread.
///
/// In this case, client should add the thread to the thread list.
/// </summary>
public class YouCompletedSoftInvitedEvent : KahlaEvent
{
    public YouCompletedSoftInvitedEvent()
    {
        Type = EventType.YouCompletedSoftInvited;
    }

    public required KahlaThreadMappedJoinedView Thread { get; init; }
}