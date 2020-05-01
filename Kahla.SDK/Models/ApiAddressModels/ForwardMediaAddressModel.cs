using System.ComponentModel.DataAnnotations;

namespace Kahla.SDK.Models.ApiAddressModels
{
    public class ForwardMediaAddressModel
    {
        [Required]
        public int SourceConversationId { get; set; }
        [Required]
        public string FileUploadDate { get; set; }
        [Required]
        public string FileName { get; set; }
        [Required]
        public int TargetConversationId { get; set; }
    }
}
