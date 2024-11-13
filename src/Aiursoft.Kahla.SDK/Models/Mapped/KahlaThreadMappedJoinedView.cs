namespace Aiursoft.Kahla.SDK.Models.Mapped;

public class KahlaThreadMappedJoinedView : KahlaThreadMappedOthersView
{
    // Top ten members.
    public required IEnumerable<KahlaUserMappedInThreadView> TopTenMembers { get; init; }

    public required MessageContext MessageContext { get; init; }

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