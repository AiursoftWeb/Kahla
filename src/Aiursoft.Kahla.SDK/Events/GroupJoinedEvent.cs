using Aiursoft.Kahla.SDK.ModelsOBS;

namespace Aiursoft.Kahla.SDK.Events;

public class GroupJoinedEvent : KahlaEvent
{
    public GroupJoinedEvent()
    {
        Type = EventType.GroupJoinedEvent;
    }

    public required GroupConversation CreatedConversation { get; set; }
    public required Message LatestMessage { get; set; }
    public int MessageCount { get; set; }
}