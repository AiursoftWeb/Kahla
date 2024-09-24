using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Aiursoft.Kahla.SDK.Services;
using Newtonsoft.Json;

namespace Aiursoft.Kahla.SDK.Models
{
    public abstract class Conversation
    {
        [Key]
        public int Id { get; set; }
        public string Discriminator { get; set; }
        
        [JsonIgnore]
        [InverseProperty(nameof(Message.Conversation))]
        public IEnumerable<Message> Messages { get; set; }

        public DateTime ConversationCreateTime { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public string DisplayName { get; set; }
        [NotMapped]
        public string DisplayImagePath { get; set; }

        public abstract KahlaUser SpecialUser(string myId);
        public abstract string GetDisplayName(string userId);
        public abstract string GetDisplayImagePath(string userId);
        public abstract int GetUnReadAmount(string userId);
        public abstract bool Muted(string userId);
        public abstract Conversation Build(string userId);
        public abstract Message GetLatestMessage();
        public abstract void ForEachUser(Action<KahlaUser, UserGroupRelation> function);
        public abstract bool HasUser(string userId);
    }

    public class UserGroupRelation
    {
        [Key]
        public int Id { get; set; }
        public DateTime JoinTime { get; set; } = DateTime.UtcNow;

        public bool Muted { get; set; }

        public string UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public KahlaUser User { get; set; }

        public int GroupId { get; set; }
        [JsonIgnore]
        [ForeignKey(nameof(GroupId))]
        public GroupConversation Group { get; set; }

        public DateTime ReadTimeStamp { get; set; } = DateTime.UtcNow;
    }
}
