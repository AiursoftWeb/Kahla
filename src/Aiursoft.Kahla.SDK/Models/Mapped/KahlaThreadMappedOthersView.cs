namespace Aiursoft.Kahla.SDK.Models.Mapped;

public class KahlaThreadMappedOthersView
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string ImagePath { get; init; }
    public required string OwnerId { get; init; }
    public required bool AllowDirectJoinWithoutInvitation { get; init; }
    public required DateTime CreateTime { get; init; }
    public required bool ImInIt { get; init; }
}