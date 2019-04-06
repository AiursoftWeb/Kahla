using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Kahla.Server.Models
{
    public class GroupConversation : Conversation
    {
        [InverseProperty(nameof(UserGroupRelation.Group))]
        public List<UserGroupRelation> Users { get; set; }
        public int GroupImageKey { get; set; }
        public string GroupName { get; set; }
        [JsonIgnore]
        public string JoinPassword { get; set; }

        [JsonProperty]
        [NotMapped]
        public bool HasPassword => !string.IsNullOrEmpty(JoinPassword);

        public string OwnerId { get; set; }
        [ForeignKey(nameof(OwnerId))]
        public KahlaUser Owner { get; set; }
        public override int GetDisplayImage(string userId) => GroupImageKey;
        public override string GetDisplayName(string userId) => GroupName;
        public override int GetUnReadAmount(string userId)
        {
            var relation = Users.SingleOrDefault(t => t.UserId == userId);
            if (relation == null)
            {
                return 0;
            }
            return Messages.Count(t => t.SendTime > relation.ReadTimeStamp);
        }

        public override Message GetLatestMessage()
        {
            try
            {
                return Messages
                    .Where(t => DateTime.UtcNow < t.SendTime + TimeSpan.FromSeconds(t.Conversation.MaxLiveSeconds))
                    .OrderByDescending(p => p.SendTime)
                    .First();
            }
            catch (InvalidOperationException)
            {
                return new Message
                {
                    Content = null,//$"You have successfully joined {this.GroupName}!",
                    SendTime = ConversationCreateTime
                };
            }
        }
    }
}
