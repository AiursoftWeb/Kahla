using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.Server.Models.ApiAddressModels
{
    public class ChangePasswordAddresModel
    {
        [Required]
        [MinLength(6)]
        [MaxLength(32)]
        public string OldPassword { get; set; }

        [Required]
        [MinLength(6)]
        [MaxLength(32)]
        public string NewPassword { get; set; }

        [Required]
        [MinLength(6)]
        [MaxLength(32)]
        [Compare(nameof(NewPassword), ErrorMessage = "The new password and confirmation password do not match.")]
        public string RepeatPassword { get; set; }
    }
}
