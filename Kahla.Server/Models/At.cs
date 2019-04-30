using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.Server.Models
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
        public int MessageId { get; set; }
        [JsonIgnore]
        [ForeignKey(nameof(MessageId))]
        public Message Message { get; set; }
    }
}
