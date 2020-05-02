using System.ComponentModel.DataAnnotations;

namespace Kahla.SDK.Models.ApiAddressModels
{
    public class ForwardMediaAddressModel
    {
        [Required]
        public int SourceConversationId { get; set; }
        [Required]
        public string SourceFilePath { get; set; }
        [Required]
        public int TargetConversationId { get; set; }
    }
}
