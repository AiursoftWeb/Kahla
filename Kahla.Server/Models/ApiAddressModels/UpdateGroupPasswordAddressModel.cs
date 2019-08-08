using Aiursoft.Pylon.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.Server.Models.ApiAddressModels
{
    public class UpdateGroupPasswordAddressModel
    {
        [Required]
        public string GroupName { get; set; }
        [NoSpace]
        [DataType(DataType.Password)]
        public string NewJoinPassword { get; set; }
    }
}
