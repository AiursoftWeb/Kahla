using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Aiursoft.Kahla.SDK.Models
{
    public class Device
    {
        public long Id { get; init; }
        public required string Name { get; init; }
        public required string IpAddress { get; init; }
        
        public required string OwnerId { get; init; }
        [JsonIgnore]
        [ForeignKey(nameof(OwnerId))]
        public KahlaUser? KahlaUser { get; init; }

        [JsonIgnore]
        public required string PushEndpoint { get; init; }
        [JsonIgnore]
        public required string PushP256Dh { get; init; }
        [JsonIgnore]
        public required string PushAuth { get; init; }
        
        public DateTime AddTime { get; init; } = DateTime.UtcNow;
    }
}
