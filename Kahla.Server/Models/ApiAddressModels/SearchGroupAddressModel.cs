using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.Server.Models.ApiAddressModels
{
    public class SearchGroupAddressModel
    {
        [MinLength(3)]
        [Required]
        public string GroupName { get; set; }
        public int Take { get; set; } = 20;
    }
}
