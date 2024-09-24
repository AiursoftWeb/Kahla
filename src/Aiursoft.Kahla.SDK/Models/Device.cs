using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Aiursoft.Kahla.SDK.Models
{
    public class Device
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public string UserId { get; set; }
        [JsonIgnore]
        [ForeignKey(nameof(UserId))]
        public KahlaUser KahlaUser { get; set; }

        [JsonIgnore]
        public string PushEndpoint { get; set; }
        [JsonIgnore]
        public string PushP256Dh { get; set; }
        [JsonIgnore]
        public string PushAuth { get; set; }
        public DateTime AddTime { get; set; } = DateTime.UtcNow;
    }
}
