using Aiursoft.Kahla.SDK.Events.Abstractions;
using Aiursoft.Kahla.SDK.Models.Mapped;

namespace Aiursoft.Kahla.SDK.Events;

/// <summary>
/// Someone sent a new message in a thread that you are in.
///
/// In this case, client should move the thread to the top of the thread list. And show the latest message in the thread.
/// </summary>
public class NewMessageEvent : KahlaEvent
{
    public NewMessageEvent()
    {
        Type = EventType.NewMessage;
    }

    public required KahlaMessageMappedSentView Message { get; init; }
    
    public required string ThreadName { get; init; }
    
    public bool Mentioned { get; set; }
}