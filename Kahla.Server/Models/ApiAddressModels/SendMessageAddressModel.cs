using System.ComponentModel.DataAnnotations;

namespace Kahla.Server.Models.ApiAddressModels
{
    public class SendMessageAddressModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [MaxLength(2500)]
        public string Content { get; set; }

        public string[] At { get; set; }
    }
}
