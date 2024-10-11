using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Aiursoft.Kahla.SDK.Models;

public class ContactRecord
{
    [Key]
    public int Id { get; set; }

    public required string CreatorId { get; set; }
    [ForeignKey(nameof(CreatorId))]
    public KahlaUser? Creator { get; set; }

    public required string TargetId { get; set; }
    [ForeignKey(nameof(TargetId))]
    [JsonIgnore]
    [NotNull]
    public KahlaUser? Target { get; set; }

    public DateTime AddTime { get; set; } = DateTime.UtcNow;
}

public class BlockRecord
{
    [Key]
    public int Id { get; set; }
    
    public required string CreatorId { get; set; }
    [ForeignKey(nameof(CreatorId))]
    public KahlaUser? Creator { get; set; }
    
    public required string TargetId { get; set; }
    [ForeignKey(nameof(TargetId))]
    [JsonIgnore]
    [NotNull]
    public KahlaUser? Target { get; set; }
    
    public DateTime AddTime { get; set; } = DateTime.UtcNow;
    
    public DateTime BlockTo { get; set; } = DateTime.MaxValue;
}