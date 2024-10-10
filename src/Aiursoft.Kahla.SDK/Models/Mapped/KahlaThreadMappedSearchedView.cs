namespace Aiursoft.Kahla.SDK.Models.Mapped;

public class KahlaThreadMappedSearchedView
{
    public required string Name { get; set; }
    public required string ImagePath { get; set; }
    public required string OwnerId { get; set; }
    public required bool AllowDirectJoinWithoutInvitation { get; set; }
}

public class KahlaThreadMappedJoinedView : KahlaThreadMappedSearchedView
{
    // Top ten members.

    // Unread amount.

    // Someone at me.

    // Muted.

    // Last message.
}