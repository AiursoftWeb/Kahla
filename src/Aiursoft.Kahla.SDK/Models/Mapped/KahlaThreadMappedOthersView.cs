namespace Aiursoft.Kahla.SDK.Models.Mapped;

public class KahlaThreadMappedOthersView
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required string ImagePath { get; set; }
    public required string OwnerId { get; set; }
    public required bool AllowDirectJoinWithoutInvitation { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime LastMessageTime { get; set; }
    public required bool ImInIt { get; set; }
}