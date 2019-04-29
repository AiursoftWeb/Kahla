using Microsoft.AspNetCore.Identity;
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

        public override async Task ForEachUserAsync(Func<KahlaUser, UserGroupRelation, Task> function, UserManager<KahlaUser> userManager)
        {
            var usersJoined = await userManager.Users
                .Include(t => t.GroupsJoined)
                .Where(t => t.GroupsJoined.Any(p => p.GroupId == Id))
                .ToListAsync();
            var taskList = new List<Task>();
            foreach (var user in usersJoined)
            {
                var task = function(user, user.GroupsJoined.FirstOrDefault(p => p.GroupId == Id));
                taskList.Add(task);
            }
            await Task.WhenAll(taskList);
        }

        public override bool IWasAted(string userId)
        {
            var relation = Users
                .SingleOrDefault(t => t.UserId == userId);
            if (relation == null || relation.User == null)
            {
                return false;
            }
            return Messages
                .Where(t => DateTime.UtcNow < t.SendTime + TimeSpan.FromSeconds(t.Conversation.MaxLiveSeconds))
                .Where(t => t.SendTime > relation.ReadTimeStamp)
                .Any(t => t.Content.Contains("@" + relation.User.NickName));
        }
    }
}
