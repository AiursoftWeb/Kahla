namespace Aiursoft.Kahla.SDK.Events
{
    public class KahlaEvent
    {
        public required EventType Type { get; set; }
        public string TypeDescription => Type.ToString();
    }
}
