namespace Aiursoft.Kahla.SDK.Events.Abstractions;

public abstract class KahlaEvent
{
    // ReSharper disable once MemberCanBeProtected.Global
    public EventType Type { get; init; }
    public string TypeDescription => Type.ToString();
}