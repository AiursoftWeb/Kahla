using Aiursoft.Kahla.SDK.ModelsOBS;

namespace Aiursoft.Kahla.SDK.Models.Mapped;

public class KahlaThreadMappedJoinedView : KahlaThreadMappedSearchedView
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


    // Someone at me. TODO.
}