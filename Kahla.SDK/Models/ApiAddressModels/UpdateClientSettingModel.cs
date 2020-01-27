namespace Kahla.SDK.Models.ApiAddressModels
{
    public class UpdateClientSettingAddressModel
    {
        public int? ThemeId { get; set; }

        public bool? EnableEmailNotification { get; set; }

        public bool? EnableEnterToSendMessage { get; set; }

        public bool? EnableInvisiable { get; set; }

        public bool? MarkEmailPublic { get; set; }
    }
}
