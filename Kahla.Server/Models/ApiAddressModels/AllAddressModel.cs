using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.Server.Models.ApiAddressModels
{
    public class AllAddressModel
    {
        [Range(1, int.MaxValue)]
        public int Take { get; set; } = 15;

        [Range(0, int.MaxValue)]
        public int Skip { get; set; } = 0;
    }
}
