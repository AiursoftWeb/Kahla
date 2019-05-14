using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.Server.Models.ApiAddressModels
{
    public class MyFriendsAddressModel
    {
        [Required]
        public bool OrderByName { get; set; } = false;

        [Range(1, 100)]
        public int Take { get; set; } = 15;

        [Range(1, int.MaxValue)]
        public int Skip { get; set; } = 0;
    }
}
