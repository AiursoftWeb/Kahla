using System.ComponentModel.DataAnnotations;

namespace Kahla.SDK.Models.ApiAddressModels
{
    public class LogOffAddressModel
    {
        [Required]
        public int DeviceId { get; set; }
    }
}
