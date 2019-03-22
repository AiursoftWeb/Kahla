using System.ComponentModel.DataAnnotations;

namespace Kahla.Server.Models.ApiAddressModels
{
    public class LogOffAddressModel
    {
        [Required]
        public int DeviceId { get; set; }
    }
}
