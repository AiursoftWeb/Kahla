using System;

namespace Kahla.Server.Models.ApiViewModels
{
    public class ContactInfo
    {
        public string DisplayName { get; set; }
        public int DisplayImageKey { get; set; }
        public string LatestMessage { get; set; }
        public DateTime LatestMessageTime { get; set; }
        public int UnReadAmount { get; set; }
        public int ConversationId { get; set; }
        public string Discriminator { get; set; }
        public string UserId { get; set; }
        public string AesKey { get; set; }
        public bool? Muted { get; set; }
        public bool SomeoneAtMe { get; set; }
    }
}
