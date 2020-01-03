using Aiursoft.SDK.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace Kahla.SDK.Models.ApiAddressModels
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
        public string Content { get; set; }

        public string[] At { get; set; }
        /// <summary>
        /// Guid. For example: 8fe1dd34-7430-4650-8b0a-8587d39dd412
        /// </summary>
        [IsGuidOrEmpty]
        [Required]
        public string MessageId { get; set; }
        /// <summary>
        /// The time when the user press the send button.
        /// </summary>
        [Required]
        public DateTime RecordTime { get; set; }
    }
}
