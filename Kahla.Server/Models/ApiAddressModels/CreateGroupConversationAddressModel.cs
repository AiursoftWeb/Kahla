using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Aiursoft.Pylon.Attributes;

namespace Kahla.Server.Models.ApiAddressModels
{
    public class CreateGroupConversationAddressModel
    {
        [Required]
        [MinLength(3)]
        [MaxLength(25)]
        [NoSpace]
        [Display(Name ="new group's name")]
        public string GroupName { get; set; }

        [MaxLength(100, ErrorMessage = "Your password was too long.")]
        [DataType(DataType.Password)]
        [NoSpace]
        public string JoinPassword { get; set; }
    }
}
