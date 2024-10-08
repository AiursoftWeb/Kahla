﻿using System.ComponentModel.DataAnnotations;
using Aiursoft.CSTools.Attributes;

namespace Aiursoft.Kahla.SDK.ModelsOBS.ApiAddressModels
{
    public class UpdateGroupPasswordAddressModel
    {
        [Required]
        public string? GroupName { get; set; }
        [NoSpace]
        [DataType(DataType.Password)]
        public string? NewJoinPassword { get; set; }
    }
}
