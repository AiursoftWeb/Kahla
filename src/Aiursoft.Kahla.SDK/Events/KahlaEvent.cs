namespace Aiursoft.Kahla.SDK.Events
{
    public class KahlaEvent
    {
        public EventType Type { get; init; }
        public string TypeDescription => Type.ToString();
    }
}
