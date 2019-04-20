using System.ComponentModel.DataAnnotations;

namespace Kahla.Server.Models.ApiAddressModels
{
    public class UpdateClientSettingAddressModel
    {
        public int? ThemeId { get; set; }

        public bool? EnableEmailNotification { get; set; }
    }
}
