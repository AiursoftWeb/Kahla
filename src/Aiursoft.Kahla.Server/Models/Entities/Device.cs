using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Aiursoft.Kahla.Server.Models.Entities;

public class Device
{
    [Key] public long Id { get; init; }

    [Required] [StringLength(200)] public required string Name { get; set; }

    [Required] [StringLength(40)] public required string IpAddress { get; init; }

    [JsonIgnore] [StringLength(64)] public required string OwnerId { get; init; }

    [JsonIgnore]
    [ForeignKey(nameof(OwnerId))]
    [NotNull]
    public KahlaUser? KahlaUser { get; init; }

    [JsonIgnore] [StringLength(400)] public required string PushEndpoint { get; set; }

    [JsonIgnore] [StringLength(400)] public required string PushP256Dh { get; set; }

    [JsonIgnore] [StringLength(150)] public required string PushAuth { get; set; }

    public DateTime AddTime { get; init; } = DateTime.UtcNow;
}