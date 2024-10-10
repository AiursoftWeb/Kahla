using System.ComponentModel.DataAnnotations;

namespace Aiursoft.Kahla.SDK.Models.AddressModels
{
    public class UpdateMeAddressModel
    {
        [StringLength(100, MinimumLength = 1)]
        public string? Bio { get; set; }
        
        [StringLength(40, MinimumLength = 1)]
        public string? NickName { get; set; }
        public int? ThemeId { get; set; }

        public bool? EnableEmailNotification { get; set; }

        public bool? EnableEnterToSendMessage { get; set; }

        public bool? EnableHideMyOnlineStatus { get; set; }

        public bool? ListInSearchResult { get; set; }
        public bool? AllowHardInvitation { get; set; }
    }
}
