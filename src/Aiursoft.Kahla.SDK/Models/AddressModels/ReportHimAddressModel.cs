using System.ComponentModel.DataAnnotations;

namespace Aiursoft.Kahla.SDK.Models.AddressModels
{
    public class ReportHimAddressModel
    {
        [Required]
        public required string TargetUserId { get; init; }

        [Required]
        [MinLength(5)]
        [MaxLength(200)]
        public required string Reason { get; init; }
    }
}
