using System.ComponentModel.DataAnnotations;

namespace Aiursoft.Kahla.SDK.Models.AddressModels
{
    public class ChangePasswordAddressModel
    {
        [Required]
        [MinLength(6, ErrorMessage = "Password length should between 6 and 32.")]
        [MaxLength(32, ErrorMessage = "Password length should between 6 and 32.")]
        [DataType(DataType.Password)]
        [Display(Name = "Old Password")]
        public string? OldPassword { get; init; }

        [Required]
        [MinLength(6, ErrorMessage = "Password length should between 6 and 32.")]
        [MaxLength(32, ErrorMessage = "Password length should between 6 and 32.")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string? NewPassword { get; init; }
    }
}
