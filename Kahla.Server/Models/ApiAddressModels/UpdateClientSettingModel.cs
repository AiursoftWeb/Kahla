using System.ComponentModel.DataAnnotations;

namespace Kahla.Server.Models.ApiAddressModels
{
    public class UpdateClientSettingAddressModel
    {
        [Required]
        [Range(0,1)]
        public int ThemeId { get; set; }
    }
}
