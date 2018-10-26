using System.ComponentModel.DataAnnotations;

namespace Kahla.Server.Models.ApiAddressModels
{
    public class UploadFileAddressModel
    {
        [Required]
        public int ConversationId { get; set; }
    }
}
