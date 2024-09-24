using System.ComponentModel.DataAnnotations;

namespace Aiursoft.Kahla.SDK.ModelsOBS.ApiAddressModels
{
    public class SendMessageAddressModel
    {
        /// <summary>
        /// Conversation id
        /// </summary>
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(2500)]
        public string? Content { get; set; }
    }
}
