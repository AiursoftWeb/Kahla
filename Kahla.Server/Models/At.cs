using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.Server.Models
{
    public class At
    {
        public int Id { get; set; }

        public string TargetUserId { get; set; }
        [ForeignKey(nameof(TargetUserId))]
        public KahlaUser TargetUser { get; set; }

        public int MessageId { get; set; }
        [ForeignKey(nameof(MessageId))]
        public Message Message { get; set; }
    }
}
