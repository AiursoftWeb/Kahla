﻿using Kahla.SDK.Services;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kahla.SDK.Models
{
    public class GroupConversation : Conversation
    {
        [InverseProperty(nameof(UserGroupRelation.Group))]
        public IEnumerable<UserGroupRelation> Users { get; set; }
        public string GroupImagePath { get; set; }
        public string GroupName { get; set; }
        [JsonIgnore]
        public string JoinPassword { get; set; }
        public bool ListInSearchResult { get; set; } = true;

        [JsonProperty]
        [NotMapped]
        public bool HasPassword => !string.IsNullOrEmpty(JoinPassword);

        public string OwnerId { get; set; }
        [JsonIgnore]
        [ForeignKey(nameof(OwnerId))]
        public KahlaUser Owner { get; set; }

        public override KahlaUser SpecialUser(string myId) => Owner;
        public override string GetDisplayName(string userId) => GroupName;
        public override string GetDisplayImagePath(string userId) => GroupImagePath;
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
            return Messages
                .Where(t => DateTime.UtcNow < t.SendTime + TimeSpan.FromSeconds(t.Conversation.MaxLiveSeconds))
                .OrderByDescending(p => p.SendTime)
                .FirstOrDefault();
        }

        public override void ForEachUser(Action<KahlaUser, UserGroupRelation> function)
        {
            foreach (var relation in Users)
            {
                function(relation.User, relation);
            }
        }

        public override bool Muted(string userId)
        {
            return Users?.SingleOrDefault(t => t.UserId == userId)?.Muted ?? throw new ArgumentNullException();
        }

        public override Conversation Build(string userId, OnlineJudger onlineJudger)
        {
            DisplayName = GetDisplayName(userId);
            DisplayImagePath = GetDisplayImagePath(userId);
            Users = Users.OrderByDescending(t => t.UserId == OwnerId).ThenBy(t => t.JoinTime);
            foreach (var user in Users)
            {
                user.User.Build(onlineJudger);
            }
            return this;
        }

        public override bool HasUser(string userId)
        {
            return Users.Any(t => t.UserId == userId);
        }
    }
}
