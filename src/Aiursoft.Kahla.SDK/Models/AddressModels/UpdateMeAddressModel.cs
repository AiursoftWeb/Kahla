using System.ComponentModel.DataAnnotations;

namespace Aiursoft.Kahla.SDK.Models.AddressModels
{
    public class UpdateMeAddressModel
    {
        [StringLength(100, MinimumLength = 1)]
        public string? Bio { get; init; }
        
        [StringLength(40, MinimumLength = 1)]
        public string? NickName { get; init; }

        public string? IconFilePath { get; init; }

        public int? ThemeId { get; init; }

        public bool? EnableEmailNotification { get; init; }

        public bool? EnableEnterToSendMessage { get; init; }

        public bool? EnableHideMyOnlineStatus { get; init; }
        public bool? AllowSearchByName { get; init; }
        public bool? AllowHardInvitation { get; init; }
    }
}
