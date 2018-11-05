using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.Server.Models.ApiAddressModels
{
    public class FileDownloadAddressAddressModel
    {
        [Required]
        public int FileKey { get; set; }
    }
}
