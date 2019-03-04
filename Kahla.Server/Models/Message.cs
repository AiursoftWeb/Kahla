using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kahla.Server.Models
{
    public class Message
    {
        public int Id { get; set; }

        public int ConversationId { get; set; }
        [JsonIgnore]
        [ForeignKey(nameof(ConversationId))]
        public Conversation Conversation { get; set; }
        public string SenderId { get; set; }
        [JsonIgnore]
        [ForeignKey(nameof(SenderId))]
        public KahlaUser Sender { get; set; }

        public DateTime SendTime { get; set; } = DateTime.UtcNow;
        public string Content { get; set; }
        public bool Read { get; set; }
    }
}
