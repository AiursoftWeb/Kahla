namespace Aiursoft.Kahla.SDK.Models.Mapped;

public class KahlaThreadMappedJoinedView : KahlaThreadMappedOthersView
{
    // Top ten members.
    public IEnumerable<KahlaUser> TopTenMembers { get; set; } = new List<KahlaUser>();

    // Unread amount.
    public int UnReadAmount { get; set; }
    
    // Last message.
    public Message? LatestMessage { get; set; }
    // Last message sender.
    public KahlaUser? LatestMessageSender { get; set; }
    
    // Muted.
    public bool Muted { get; set; }

    // I'm an Admin of this thread.
    public bool ImAdmin { get; set; }
    
    // I'm the owner of this thread.
    public bool ImOwner { get; set; }
    
    // Someone at me. TODO.
}