using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Kahla.Server.Models.ApiAddressModels
{
    public class UploadFileAddressModel
    {
        [Required]
        [FromQuery]
        public int ConversationId { get; set; }
    }
}
