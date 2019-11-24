using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Kahla.SDK.Models.ApiAddressModels
{
    public class InitFileUpload
    {
        [Required]
        [FromQuery]
        public int ConversationId { get; set; }
    }
}
