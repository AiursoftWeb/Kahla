using System.ComponentModel.DataAnnotations;

namespace Kahla.SDK.Models.ApiAddressModels
{
    public class FileDownloadAddressAddressModel
    {
        [Required]
        public int FileKey { get; set; }
    }
}
