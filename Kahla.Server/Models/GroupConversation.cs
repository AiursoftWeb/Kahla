using Kahla.Server.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.Server.Models
{
    public class GroupConversation : Conversation
    {
        [InverseProperty(nameof(UserGroupRelation.Group))]
        public IEnumerable<UserGroupRelation> UserRelations { get; set; }
        public string GroupImagePath { get; set; }
        public string GroupName { get; set; }
        [JsonIgnore]
        public string JoinPassword { get; set; }

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
            var relation = UserRelations.SingleOrDefault(t => t.UserId == userId);
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

        public override async Task ForEachUserAsync(Func<KahlaUser, UserGroupRelation, Task> function)
        {
            var taskList = new List<Task>();
            foreach (var relation in UserRelations)
            {
                var task = function(relation.User, relation);
                taskList.Add(task);
            }
            await Task.WhenAll(taskList);
        }

        public override bool WasAted(string userId)
        {
            return Messages
                .Where(t => DateTime.UtcNow < t.SendTime + TimeSpan.FromSeconds(t.Conversation.MaxLiveSeconds))
                .Where(t => t.SendTime > UserRelations.SingleOrDefault(p => p.UserId == userId).ReadTimeStamp)
                .Any(t => t.Ats.Any(p => p.TargetUserId == userId));
        }

        public override bool Muted(string userId)
        {
            return UserRelations.SingleOrDefault(t => t.UserId == userId).Muted;
        }

        public override async Task<DateTime> SetLastRead(KahlaDbContext dbContext, string userId)
        {
            var relation = await dbContext.UserGroupRelations
                    .SingleOrDefaultAsync(t => t.UserId == userId && t.GroupId == Id);
            try
            {
                return relation.ReadTimeStamp;
            }
            finally
            {
                relation.ReadTimeStamp = DateTime.UtcNow;
            }
        }

        public override Conversation Build(string userId)
        {
            DisplayName = GetDisplayName(userId);
            DisplayImagePath = GetDisplayImagePath(userId);
            UserRelations = UserRelations.OrderByDescending(t => t.UserId == OwnerId).ThenBy(t => t.JoinTime);
            return this;
        }

        public override bool HasUser(string userId)
        {
            return UserRelations.Any(t => t.UserId == userId);
        }
    }
}
