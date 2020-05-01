namespace Kahla.SDK.Models.ApiAddressModels
{
    public class ForwardMediaAddressModel
    {
        public int SourceConversationId { get; set; }
        public string FileUploadDate { get; set; }
        public string FileName { get; set; }
        public int TargetConversationId { get; set; }
    }
}
