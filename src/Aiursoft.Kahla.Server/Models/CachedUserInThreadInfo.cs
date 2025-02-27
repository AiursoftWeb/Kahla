namespace Aiursoft.Kahla.Server.Models;

public class CachedUserInThreadInfo
{
    public required string UserId { get; init; }
    public required int UnreadAmountSinceBoot { get; set; }
    public required bool Muted { get; set; }
    public required bool BeingAted { get; set; }
}