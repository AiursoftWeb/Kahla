using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Aiursoft.Kahla.SDK.Models.Entities;

public class BlockRecord
{
    [Key]
    public int Id { get; init; }
    
    public required string CreatorId { get; init; }
    [ForeignKey(nameof(CreatorId))]
    [NotNull]
    public KahlaUser? Creator { get; init; }
    
    public required string TargetId { get; init; }
    [ForeignKey(nameof(TargetId))]
    [JsonIgnore]
    [NotNull]
    public KahlaUser? Target { get; init; }
    
    public DateTime AddTime { get; init; } = DateTime.UtcNow;
}