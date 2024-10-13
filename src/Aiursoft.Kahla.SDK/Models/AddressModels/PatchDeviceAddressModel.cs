using System.ComponentModel.DataAnnotations;

namespace Aiursoft.Kahla.SDK.Models.AddressModels
{
    public class PatchDeviceAddressModel
    {
        [Required]
        [StringLength(30, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
        public string? Name { get; init; }
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
        public string? PushEndpoint { get; init; }
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
        public string? PushP256Dh { get; init; }
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
        public string? PushAuth { get; init; }
    }
}
