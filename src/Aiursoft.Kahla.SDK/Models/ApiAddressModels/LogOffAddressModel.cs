using System.ComponentModel.DataAnnotations;

namespace Aiursoft.Kahla.SDK.Models.ApiAddressModels
{
    public class LogOffAddressModel
    {
        [Required]
        public int DeviceId { get; set; }
    }
}
