using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Aiursoft.Kahla.SDK.ModelsOBS
{
    public class Request
    {
        [Key]
        public int Id { get; set; }

        public required string CreatorId { get; set; }
        [ForeignKey(nameof(CreatorId))]
        public KahlaUser? Creator { get; set; }

        public required string TargetId { get; set; }
        [ForeignKey(nameof(TargetId))]
        [JsonIgnore]
        public KahlaUser? Target { get; set; }

        public DateTime CreateTime { get; set; } = DateTime.UtcNow;
        public bool Completed { get; set; }
    }
}
