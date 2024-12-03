using System.Text.Json;
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
    YouCompletedSoftInvited = 20

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
public class YouCompletedSoftInvitedEvent : KahlaEvent
{
    public YouCompletedSoftInvitedEvent()
    {
        Type = EventType.YouCompletedSoftInvited;
    }

    public required KahlaThreadMappedJoinedView Thread { get; init; }
}

public static class JsonTools
{
    public static KahlaEvent DeseralizeKahlaEvent(string json)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true // 忽略属性名称的大小写
        };
        
        using var jsonDocument = JsonDocument.Parse(json);
        var root = jsonDocument.RootElement;

        // Check if the 'Type' property exists
        if (root.TryGetProperty("type", out var typeElement))
        {
            // Get the integer value of the 'Type' property
            var typeValue = typeElement.GetInt32();
            var eventType = (EventType)typeValue;

            // Map the event type to the corresponding class type
            var targetType = eventType switch
            {
                EventType.NewMessage => typeof(NewMessageEvent),
                EventType.ThreadDissolved => typeof(ThreadDissolvedEvent),
                EventType.YouBeenKicked => typeof(YouBeenKickedEvent),
                EventType.YouLeft => typeof(YouLeftEvent),
                EventType.CreateScratched => typeof(CreateScratchedEvent),
                EventType.YouDirectJoined => typeof(YouDirectJoinedEvent),
                EventType.YourHardInviteFinished => typeof(YourHardInviteFinishedEvent),
                EventType.YouWasHardInvited => typeof(YouWasHardInvitedEvent),
                EventType.YouCompletedSoftInvited => typeof(YouCompletedSoftInvitedEvent),
                _ => typeof(KahlaEvent) // Default to base class if type is unknown
            };

            // Deserialize the JSON into the target type
            var kahlaEvent = (KahlaEvent)JsonSerializer.Deserialize(json, targetType, options)!;
            return kahlaEvent;
        }
        else
        {
            throw new JsonException("The 'Type' property was not found in the JSON.");
        }
    }
}