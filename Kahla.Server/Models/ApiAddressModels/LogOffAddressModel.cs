using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.Server.Models.ApiAddressModels
{
    public class LogOffAddressModel
    {
        [Required]
        public int DeviceId { get; set; }
    }
}
