using Aiursoft.Kahla.SDK.Models.Mapped;

namespace Aiursoft.Kahla.SDK.Events;

public abstract class KahlaEvent
{
    // ReSharper disable once MemberCanBeProtected.Global
    public EventType Type { get; init; }
    public string TypeDescription => Type.ToString();
}

public enum EventType
{
    #region Events that means the thread's latest status has changed.

    /// <summary>
    /// Someone sent a new message in a thread that you are in.
    ///
    /// In this case, client should move the thread to the top of the thread list. And show the latest message in the thread.
    /// </summary>
    NewMessage = 0,

    #endregion

    #region Events that means you are no longer in the thread.

    /// <summary>
    /// A thread that you are in has been dissolved.
    ///
    /// In this case, client should remove the thread from the thread list.
    /// </summary>
    ThreadDissolved = 8,

    /// <summary>
    /// You have been kicked from a thread.
    ///
    /// In this case, client should remove the thread from the thread list.
    /// </summary>
    YouBeenKicked = 9,

    /// <summary>
    /// You have left a thread.
    ///
    /// In this case, client should remove the thread from the thread list.
    /// </summary>
    YouLeft = 10,

    #endregion

    // Events that means a new thread has been created.

    #region Events that means a new thread should appear on the thread list.

    /// <summary>
    /// A new thread has been created by you.
    ///
    /// In this case, client should add the thread to the thread list.
    /// </summary>
    CreateScratched = 16,

    /// <summary>
    /// You have been directly invited to a thread.
    ///
    /// In this case, client should add the thread to the thread list.
    /// </summary>
    YouDirectJoined = 17,

    /// <summary>
    /// You hard invited someone to a thread.
    ///
    /// In this case, client should add the thread to the thread list.
    /// </summary>
    YourHardInviteFinished = 18,

    /// <summary>
    /// You were hard invited to a thread.
    ///
    /// In this case, client should add the thread to the thread list.
    /// </summary>
    YouWasHardInvited = 19,

    /// <summary>
    /// You completed a software invite and soft joined a thread.
    ///
    /// In this case, client should add the thread to the thread list.
    /// </summary>
    YouCompletedSoftwareInvited = 20

    #endregion
}

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

    public bool Muted { get; set; }
    public required KahlaMessageMappedSentView Message { get; init; }
    
    /// <summary>
    /// TODO: Finish the mentioned feature.
    /// </summary>
    public bool Mentioned { get; set; }
}

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

/// <summary>
/// You completed a software invite and soft joined a thread.
///
/// In this case, client should add the thread to the thread list.
/// </summary>
public class YouCompletedSoftwareInvitedEvent : KahlaEvent
{
    public YouCompletedSoftwareInvitedEvent()
    {
        Type = EventType.YouCompletedSoftwareInvited;
    }

    public required KahlaThreadMappedJoinedView Thread { get; init; }
}
