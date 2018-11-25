using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Kahla.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace Kahla.Server.Models
{
    public interface IConversation
    {
        string GetDisplayName(string userId);
        int GetDisplayImage(string userId);
        int GetUnReadAmount(string userId);
        Message GetLatestMessage();
    }
    public abstract class Conversation : IConversation
    {
        [Key]
        public int Id { get; set; }

        public string Discriminator { get; set; }

        public string AESKey { get; set; }
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

        public bool Blocked { get; set; }

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
