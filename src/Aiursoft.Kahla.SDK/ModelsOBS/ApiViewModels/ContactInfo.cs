using Aiursoft.Kahla.SDK.Models;
using Newtonsoft.Json;

namespace Aiursoft.Kahla.SDK.ModelsOBS.ApiViewModels
{
    public class ContactInfo
    {
        public required string DisplayName { get; set; }
        public required string DisplayImagePath { get; set; }
        public required virtual Message LatestMessage { get; set; }
        public int UnReadAmount { get; set; }
        public int ConversationId { get; set; }
        public required string Discriminator { get; set; }
        public required string UserId { get; set; }
        public bool Muted { get; set; }
        public bool? Online { get; set; }
        [JsonIgnore]
        public KahlaUser? Sender { get; set; }
        [JsonIgnore]
        public List<Message> Messages { get; set; } = new();
    }
}
