using System.ComponentModel.DataAnnotations;

namespace Aiursoft.Kahla.SDK.Models.ApiAddressModels
{
    public class UpdateInfoAddressModel
    {
        [Required]
        [MaxLength(20)]
        public virtual string NickName { get; set; }
        [MaxLength(80)]
        public virtual string Bio { get; set; }
        [Required]
        public virtual string HeadIconPath { get; set; }
    }
}
