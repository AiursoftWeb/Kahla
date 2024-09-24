using System.ComponentModel.DataAnnotations;

namespace Aiursoft.Kahla.SDK.Models.ApiAddressModels
{
    public class ReportHimAddressModel
    {
        [Required]
        public string TargetUserId { get; set; }

        [Required]
        [MinLength(5)]
        [MaxLength(200)]
        public string Reason { get; set; }
    }
}
