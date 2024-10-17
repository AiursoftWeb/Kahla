using System.ComponentModel.DataAnnotations;

namespace Aiursoft.Kahla.SDK.Models.Entities;

public class Message
{
    [Key]
    public int Id { get; init; }
    
    public required Guid MessageId { get; init; } = Guid.NewGuid();
    public required string Content { get; init; }
    public required DateTime SendTime { get; init; } = DateTime.UtcNow;
    
    public required string SenderId { get; init; }
    
    public required int ThreadId { get; init; }
}