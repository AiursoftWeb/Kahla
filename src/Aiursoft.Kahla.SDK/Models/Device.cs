using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Aiursoft.Kahla.SDK.Models
{
    public class Device
    {
        public long Id { get; set; }
        public required string Name { get; set; }
        public required string IpAddress { get; set; }
        public required string UserId { get; set; }
        [JsonIgnore]
        [ForeignKey(nameof(UserId))]
        public required KahlaUser KahlaUser { get; set; }

        [JsonIgnore]
        public required string PushEndpoint { get; set; }
        [JsonIgnore]
        public required string PushP256Dh { get; set; }
        [JsonIgnore]
        public required string PushAuth { get; set; }
        public DateTime AddTime { get; set; } = DateTime.UtcNow;
    }
}
