using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kahla.SDK.Models
{
    public class At
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string TargetUserId { get; set; }
        [JsonIgnore]
        [ForeignKey(nameof(TargetUserId))]
        public KahlaUser TargetUser { get; set; }

        [JsonIgnore]
        public Guid MessageId { get; set; }
        [JsonIgnore]
        [ForeignKey(nameof(MessageId))]
        public Message Message { get; set; }
    }
}
