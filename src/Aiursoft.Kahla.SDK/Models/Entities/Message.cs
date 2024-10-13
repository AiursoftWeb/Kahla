using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Aiursoft.Kahla.SDK.Models.Entities
{
    public class Message
    {
        [Key]
        public Guid Id { get; init; }

        public required int ThreadId { get; init; }
        [JsonIgnore]
        [ForeignKey(nameof(ThreadId))]
        [NotNull]
        public ChatThread? Thread { get; init; }
        
        public required string SenderId { get; init; }
        [ForeignKey(nameof(SenderId))]
        [NotNull]
        public KahlaUser? Sender { get; init; }

        public DateTime SendTime { get; init; } = DateTime.UtcNow;
        
        [StringLength(16384)]
        public required string Content { get; init; }
        public bool GroupWithPrevious { get; init; }
    }
}
