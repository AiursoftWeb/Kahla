using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.Server.Models.ApiAddressModels
{
    public class SearchFriendsAddressModel
    {
        [MinLength(3)]
        [Required]
        public string NickName { get; set; }

        public int Take { get; set; } = 20;
    }
}
