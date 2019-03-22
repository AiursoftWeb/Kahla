using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kahla.Server.Models.ApiAddressModels
{
    public class UpdateClientSettingAddressModel
    {
        [Required]
        [Range(0,1)]
        public int ThemeId { get; set; }
    }
}
