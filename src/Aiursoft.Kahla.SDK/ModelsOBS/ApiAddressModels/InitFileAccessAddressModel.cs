using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Aiursoft.Kahla.SDK.ModelsOBS.ApiAddressModels
{
    public class InitFileAccessAddressModel
    {
        [Required]
        [FromQuery(Name = "conversationId")]
        public int ConversationId { get; set; }

        [FromQuery(Name = "upload")]
        public bool Upload { get; set; }

        [FromQuery(Name = "download")]
        public bool Download { get; set; }
    }
}
