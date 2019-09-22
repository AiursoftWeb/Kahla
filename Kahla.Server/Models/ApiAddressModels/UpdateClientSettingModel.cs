namespace Kahla.Server.Models.ApiAddressModels
{
    public class UpdateClientSettingAddressModel
    {
        public int? ThemeId { get; set; }

        public bool? EnableEmailNotification { get; set; }

        public bool? EnableEnterToSendMessage { get; set; }
    }
}
