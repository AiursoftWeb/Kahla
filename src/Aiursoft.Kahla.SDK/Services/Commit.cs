namespace Aiursoft.Kahla.SDK.Services;

public class Commit<T>
{
    public string Id { get; init; } = Guid.NewGuid().ToString("D");
    public required T Item { get; init; }
    public DateTime CommitTime { get; set; } = DateTime.UtcNow;

    public override string ToString()
    {
        return $"{Item}";
    }
}