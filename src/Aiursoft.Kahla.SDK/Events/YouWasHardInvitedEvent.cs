using Aiursoft.Kahla.SDK.Events.Abstractions;
using Aiursoft.Kahla.SDK.Models.Mapped;

namespace Aiursoft.Kahla.SDK.Events;

/// <summary>
/// You were hard invited to a thread.
///
/// In this case, client should add the thread to the thread list.
/// </summary>
public class YouWasHardInvitedEvent : KahlaEvent
{
    public YouWasHardInvitedEvent()
    {
        Type = EventType.YouWasHardInvited;
    }

    public required KahlaThreadMappedJoinedView Thread { get; init; }
}