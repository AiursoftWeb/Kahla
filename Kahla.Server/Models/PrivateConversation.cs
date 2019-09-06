using Kahla.Server.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.Server.Models
{
    public class PrivateConversation : Conversation
    {
        public string RequesterId { get; set; }
        [ForeignKey(nameof(RequesterId))]
        public KahlaUser RequestUser { get; set; }

        public string TargetId { get; set; }
        [ForeignKey(nameof(TargetId))]
        public KahlaUser TargetUser { get; set; }
        [NotMapped]
        // Only a property for convenience.
        public string AnotherUserId { get; set; }

        public override KahlaUser SpecialUser(string myId) => myId == RequesterId ? TargetUser : RequestUser;
        public override string GetDisplayImagePath(string userId) => SpecialUser(userId).IconFilePath;
        public override string GetDisplayName(string userId) => SpecialUser(userId).NickName;
        public override int GetUnReadAmount(string userId) => Messages.Count(p => !p.Read && p.SenderId != userId);


        public override Message GetLatestMessage()
        {
            return Messages
                .Where(t => DateTime.UtcNow < t.SendTime + TimeSpan.FromSeconds(t.Conversation.MaxLiveSeconds))
                .OrderByDescending(p => p.SendTime)
                .FirstOrDefault();
        }

        public override async Task ForEachUserAsync(Func<KahlaUser, UserGroupRelation, Task> function, UserManager<KahlaUser> userManager)
        {
            var taskList = new List<Task>();
            var requester = await userManager.FindByIdAsync(RequesterId);
            taskList.Add(function(requester, null));
            if (RequesterId != TargetId)
            {
                var targetUser = await userManager.FindByIdAsync(TargetId);
                taskList.Add(function(targetUser, null));
            }
            await Task.WhenAll(taskList);
        }

        public override bool WasAted(string userId)
        {
            return false;
        }

        public override bool Muted(string userId)
        {
            return false;
        }

        public async override Task<DateTime> SetLastRead(KahlaDbContext dbContext, string userId)
        {
            var query = dbContext.Messages
                .Where(t => t.ConversationId == this.Id)
                .Where(t => t.SenderId != userId);
            try
            {
                return (await query
                    .Where(t => t.Read)
                    .OrderByDescending(t => t.SendTime)
                    .FirstOrDefaultAsync())
                    ?.SendTime ?? DateTime.MinValue;
            }
            finally
            {
                await query
                    .Where(t => t.Read == false)
                    .ForEachAsync(t => t.Read = true);
            }
        }

        public override Conversation Build(string userId)
        {
            DisplayName = GetDisplayName(userId);
            DisplayImagePath = GetDisplayImagePath(userId);
            AnotherUserId = SpecialUser(userId).Id;
            return this;
        }

        public override bool HasUser(string userId)
        {
            return RequesterId == userId || TargetId == userId;
        }
    }
}
