using Newtonsoft.Json;

namespace Aiursoft.Kahla.SDK.Models.ApiViewModels
{
    public class ContactInfo
    {
        public string DisplayName { get; set; }
        public string DisplayImagePath { get; set; }
        public virtual Message LatestMessage { get; set; }
        public int UnReadAmount { get; set; }
        public int ConversationId { get; set; }
        public string Discriminator { get; set; }
        public string UserId { get; set; }
        public bool Muted { get; set; }
        public bool SomeoneAtMe { get; set; }
        public bool? Online { get; set; }
        [JsonIgnore]
        public KahlaUser Sender { get; set; }
        [JsonIgnore]
        public List<Message> Messages { get; set; } = new();
    }
}
