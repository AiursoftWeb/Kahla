using System.ComponentModel.DataAnnotations;

namespace Aiursoft.Kahla.SDK.Models.AddressModels;

public class SignInAddressModel
{
    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Required]
    [MinLength(6, ErrorMessage = "Password length should between 6 and 32.")]
    [MaxLength(32, ErrorMessage = "Password length should between 6 and 32.")]
    [DataType(DataType.Password)]
    public required string Password { get; init; }
}