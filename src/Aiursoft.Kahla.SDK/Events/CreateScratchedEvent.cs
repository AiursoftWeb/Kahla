using Aiursoft.Kahla.SDK.Events.Abstractions;
using Aiursoft.Kahla.SDK.Models.Mapped;

namespace Aiursoft.Kahla.SDK.Events;

/// <summary>
/// A new thread has been created by you.
///
/// In this case, client should add the thread to the thread list.
/// </summary>
public class CreateScratchedEvent : KahlaEvent
{
    public CreateScratchedEvent()
    {
        Type = EventType.CreateScratched;
    }

    public required KahlaThreadMappedJoinedView Thread { get; init; }
}