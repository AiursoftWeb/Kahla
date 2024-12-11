using Aiursoft.Kahla.SDK.Events.Abstractions;
using Aiursoft.Kahla.SDK.Models.Mapped;

namespace Aiursoft.Kahla.SDK.Events;

/// <summary>
/// You hard invited someone to a thread.
///
/// In this case, client should add the thread to the thread list.
/// </summary>
public class YourHardInviteFinishedEvent : KahlaEvent
{
    public YourHardInviteFinishedEvent()
    {
        Type = EventType.YourHardInviteFinished;
    }

    public required KahlaThreadMappedJoinedView Thread { get; init; }
}