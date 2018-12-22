using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.Server.Models
{
    public class GroupConversation : Conversation, IConversation
    {
        [InverseProperty(nameof(UserGroupRelation.Group))]
        public List<UserGroupRelation> Users { get; set; }
        public int GroupImageKey { get; set; }
        public string GroupName { get; set; }
        [JsonIgnore]
        public string JoinPassword { get; set; }

        [JsonProperty]
        [NotMapped]
        public bool HasPassword => !string.IsNullOrEmpty(this.JoinPassword);

        public string OwnerId { get; set; }
        [ForeignKey(nameof(OwnerId))]
        public KahlaUser Owner { get; set; }
        public override int GetDisplayImage(string userId) => GroupImageKey;
        public override string GetDisplayName(string userId) => GroupName;
        public override int GetUnReadAmount(string userId)
        {
            var relation = Users.SingleOrDefault(t => t.UserId == userId);
            return Messages.Where(t => t.SendTime > relation.ReadTimeStamp).Count();
        }

        public override Message GetLatestMessage()
        {
            try
            {
                return this.Messages.OrderByDescending(p => p.SendTime).First();
            }
            catch (InvalidOperationException)
            {
                return new Message
                {
                    Content = null,//$"You have successfully joined {this.GroupName}!",
                    SendTime = this.ConversationCreateTime
                };
            }
        }
    }
}
