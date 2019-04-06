using System.ComponentModel.DataAnnotations;

namespace Kahla.Server.Models.ApiAddressModels
{
    public class UpdateClientSettingAddressModel
    {
        [Required]
        public int ThemeId { get; set; }
    }
}
