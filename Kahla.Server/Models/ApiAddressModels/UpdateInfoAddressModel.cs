using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.Server.Models.ApiAddressModels
{
    public class UpdateInfoAddressModel
    {
        [Required]
        [MaxLength(20)]
        public virtual string NickName { get; set; }
        [MaxLength(80)]
        public virtual string Bio { get; set; }
        public virtual int HeadImgKey { get; set; }
    }
}
