﻿using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Kahla.SDK.Models.ApiAddressModels
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
