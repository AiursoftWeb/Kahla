using Aiursoft.Kahla.SDK.Models.Entities;

namespace Aiursoft.Kahla.SDK.Models.Mapped;

public class KahlaThreadMappedJoinedView : KahlaThreadMappedOthersView
{
    // Top ten members.
    public required IEnumerable<KahlaUser> TopTenMembers { get; set; } = new List<KahlaUser>();

    // Unread amount.
    public required int UnReadAmount { get; set; }
    
    // Last message.
    public required Message? LatestMessage { get; set; }
    // Last message sender.
    public required KahlaUser? LatestMessageSender { get; set; }
    // If Last message is null, this is the last message time. Or it is the creation time of this thread.
    public required DateTime LastMessageTime { get; init; }
    // Muted.
    public required bool Muted { get; init; }

    // I'm an Admin of this thread.
    public required bool ImAdmin { get; init; }
    
    // I'm the owner of this thread.
    public required bool ImOwner { get; init; }
    
    // Thread properties.
    public required bool AllowSearchByName { get; init; }
    
    public required bool AllowMembersSendMessages { get; init; }
    
    public required bool AllowMembersEnlistAllMembers { get; init; }
    
    public required bool AllowMemberSoftInvitation { get; init; }
    
    // Someone at me. TODO.
}