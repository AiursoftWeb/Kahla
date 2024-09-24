using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Aiursoft.Kahla.SDK.Models
{
    public class Device
    {
        [Key]
        public long Id { get; init; }
        [Required]
        [StringLength(30)]
        public required string Name { get; set; }
        [Required]
        [StringLength(40)]
        public required string IpAddress { get; init; }
        
        [JsonIgnore]
        public required string OwnerId { get; init; }
        [JsonIgnore]
        [ForeignKey(nameof(OwnerId))]
        public KahlaUser? KahlaUser { get; init; }

        [JsonIgnore]
        [StringLength(400)]
        public required string PushEndpoint { get; set; }
        [JsonIgnore]
        [StringLength(400)]
        public required string PushP256Dh { get; set; }
        [JsonIgnore]
        [StringLength(150)]
        public required string PushAuth { get; set; }
        
        public DateTime AddTime { get; init; } = DateTime.UtcNow;
    }
}
