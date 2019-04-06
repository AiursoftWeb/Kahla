using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kahla.Server.Models
{
    public abstract class Conversation
    {
        [Key]
        public int Id { get; set; }
        public string Discriminator { get; set; }
        public string AESKey { get; set; }
        public int MaxLiveSeconds { get; set; } = int.MaxValue;
        [JsonIgnore]
        [InverseProperty(nameof(Message.Conversation))]
        public IEnumerable<Message> Messages { get; set; }

        public DateTime ConversationCreateTime { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public string DisplayName { get; set; }
        [NotMapped]
        public int DisplayImage { get; set; }


        public abstract string GetDisplayName(string userId);
        public abstract int GetDisplayImage(string userId);
        public abstract int GetUnReadAmount(string userId);
        public abstract Message GetLatestMessage();
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
