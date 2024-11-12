namespace Aiursoft.Kahla.SDK.Models.Mapped;

public class MessageContext
{
    // Unread amount.
    public required uint UnReadAmount { get; init; }
    // Last message.
    public required KahlaMessageMappedSentView? LatestMessage { get; init; }
}