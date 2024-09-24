using System.ComponentModel.DataAnnotations;

namespace Aiursoft.Kahla.SDK.Models.ApiAddressModels
{
    public class FileDownloadAddressAddressModel
    {
        [Required]
        public int FileKey { get; set; }
    }
}
