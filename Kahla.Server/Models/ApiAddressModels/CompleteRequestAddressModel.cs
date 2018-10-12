using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.Server.Models.ApiAddressModels
{
    public class CompleteRequestAddressModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public bool Accept { get; set; }
    }
}
