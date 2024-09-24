using System.ComponentModel.DataAnnotations;

namespace Aiursoft.Kahla.SDK.Models.ApiAddressModels
{
    public class ChangePasswordAddressModel
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
