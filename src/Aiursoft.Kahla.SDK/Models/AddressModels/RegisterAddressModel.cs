using System.ComponentModel.DataAnnotations;

namespace Aiursoft.Kahla.SDK.Models.AddressModels;

public class RegisterAddressModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Required]
    [MinLength(6, ErrorMessage = "Password length should between 6 and 32.")]
    [MaxLength(32, ErrorMessage = "Password length should between 6 and 32.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string? Password { get; set; }
}