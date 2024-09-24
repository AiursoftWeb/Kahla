using System.ComponentModel.DataAnnotations;

namespace Aiursoft.Kahla.SDK.Models.ApiAddressModels
{
    public class UpdateDeviceAddressModel
    {
        [Required]
        public long DeviceId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string PushEndpoint { get; set; }
        [Required]
        public string PushP256Dh { get; set; }
        [Required]
        public string PushAuth { get; set; }
    }
}
