using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Aiursoft.Kahla.SDK.Models
{
    public class Message
    {
        public Guid Id { get; set; }

        public int ConversationId { get; set; }
        [JsonIgnore]
        [ForeignKey(nameof(ConversationId))]
        public Conversation? Conversation { get; set; }
        
        public required string SenderId { get; set; }
        [ForeignKey(nameof(SenderId))]
        public KahlaUser? Sender { get; set; }

        public DateTime SendTime { get; set; } = DateTime.UtcNow;
        public required string Content { get; set; }
        public bool Read { get; set; }
        public bool GroupWithPrevious { get; set; }
    }
}
