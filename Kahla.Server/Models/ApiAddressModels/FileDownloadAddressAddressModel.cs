using System.ComponentModel.DataAnnotations;

namespace Kahla.Server.Models.ApiAddressModels
{
    public class FileDownloadAddressAddressModel
    {
        [Required]
        public int FileKey { get; set; }
    }
}
