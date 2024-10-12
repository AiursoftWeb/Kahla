using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Aiursoft.Kahla.SDK.Models
{
    [Obsolete]
    public class Request
    {
        [Key]
        public int Id { get; set; }

        public required string CreatorId { get; set; }
        [ForeignKey(nameof(CreatorId))]
        [NotNull]
        public KahlaUser? Creator { get; set; }

        public required string TargetId { get; set; }
        [ForeignKey(nameof(TargetId))]
        [JsonIgnore]
        [NotNull]
        public KahlaUser? Target { get; set; }

        public DateTime CreateTime { get; set; } = DateTime.UtcNow;
        public bool Completed { get; set; }
    }
}
