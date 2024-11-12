namespace Aiursoft.Kahla.SDK.Models.Mapped;

public class DeviceMappedOwnerView
{
    public required long Id { get; init; }

    public required string Name { get; init; }

    public required string IpAddress { get; init; }

    public required DateTime AddTime { get; init; } = DateTime.UtcNow;
}