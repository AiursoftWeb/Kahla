using System.ComponentModel.DataAnnotations;

namespace Aiursoft.Kahla.SDK.Models.AddressModels;

public class SignInAddressModel
{
    [Required]
    [EmailAddress]
    public string? Email { get; set; }

    [Required]
    [MinLength(6, ErrorMessage = "Password length should between 6 and 32.")]
    [MaxLength(32, ErrorMessage = "Password length should between 6 and 32.")]
    [DataType(DataType.Password)]
    public string? Password { get; set; }
}