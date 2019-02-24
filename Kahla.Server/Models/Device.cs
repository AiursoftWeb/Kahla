using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kahla.Server.Models
{
    public class Device
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string UserID { get; set; }
        [JsonIgnore]
        [ForeignKey(nameof(UserID))]
        public KahlaUser KahlaUser { get; set; }

        public string PushEndpoint { get; set; }
        public string PushP256DH { get; set; }
        public string PushAuth { get; set; }
        public DateTime AddTime { get; set; } = DateTime.UtcNow;
    }
}
