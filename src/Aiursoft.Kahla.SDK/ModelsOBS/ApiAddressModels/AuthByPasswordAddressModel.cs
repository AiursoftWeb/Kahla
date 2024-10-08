﻿using System.ComponentModel.DataAnnotations;

namespace Aiursoft.Kahla.SDK.ModelsOBS.ApiAddressModels
{
    public class AuthByPasswordAddressModel
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
        [Required]
        public string? Password { get; set; }
    }
}
