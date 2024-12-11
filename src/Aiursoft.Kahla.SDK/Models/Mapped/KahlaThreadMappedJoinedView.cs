namespace Aiursoft.Kahla.SDK.Models.Mapped;

public class KahlaThreadMappedJoinedView : KahlaThreadMappedOthersView
{
    // Top ten members.
    public required IEnumerable<KahlaUserMappedInThreadView> TopTenMembers { get; init; }

    public required MessageContext MessageContext { get; init; }

    /// <summary>
    /// If I have muted this thread.
    /// </summary>
    public required bool Muted { get; init; }

    /// <summary>
    /// If I'm an admin of this thread.
    /// </summary>
    public required bool ImAdmin { get; init; }

    /// <summary>
    /// If I'm the owner of this thread.
    /// </summary>
    public required bool ImOwner { get; init; }
    
    /// <summary>
    /// Exists a message which is unread by me and at me.
    /// </summary>
    public required bool UnreadAtMe { get; init; }

    // Thread properties.
    public required bool AllowSearchByName { get; init; }

    public required bool AllowMembersSendMessages { get; init; }

    public required bool AllowMembersEnlistAllMembers { get; init; }

    public required bool AllowMemberSoftInvitation { get; init; }
    
    public required uint TotalMessages { get; init; }
}