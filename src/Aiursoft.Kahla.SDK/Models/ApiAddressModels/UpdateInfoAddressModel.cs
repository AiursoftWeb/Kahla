using System.ComponentModel.DataAnnotations;

namespace Aiursoft.Kahla.SDK.Models.ApiAddressModels
{
    public class UpdateInfoAddressModel
    {
        [Required]
        [MaxLength(20)]
        public string? NickName { get; set; }
        [MaxLength(80)]
        public string? Bio { get; set; }
        [Required]
        public string? HeadIconPath { get; set; }
    }
}
