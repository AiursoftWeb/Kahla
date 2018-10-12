using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.Server.Models
{
    public class Credential
    {
        public int Id { get; set; }
        public string Value { get; set; }

        public string UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public KahlaUser User { get; set; }
    }
}
