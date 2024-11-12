namespace Aiursoft.Kahla.SDK.Models.Mapped;

public class KahlaUserMappedPublicView
{
    public required string Id { get; init; }
    
    public required string? NickName { get; init; }
    
    public required string? Bio { get; init; }

    public required string? IconFilePath { get; init; }

    public required DateTime AccountCreateTime { get; init; }

    public required bool EmailConfirmed { get; init; }
    public required string? Email { get; init; }
}