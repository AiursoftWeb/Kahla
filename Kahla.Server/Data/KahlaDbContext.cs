using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Kahla.Server.Models;
using Aiursoft.Pylon.Services;
using Aiursoft.Pylon;

namespace Kahla.Server.Data
{
    public class KahlaDbContext : IdentityDbContext<KahlaUser>
    {
        public KahlaDbContext(DbContextOptions<KahlaDbContext> options) : base(options)
        {

        }

        public DbSet<Message> Messages { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<PrivateConversation> PrivateConversations { get; set; }
        public DbSet<GroupConversation> GroupConversations { get; set; }
        public DbSet<UserGroupRelation> UserGroupRelations { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<FileRecord> FileRecords { get; set; }
        public DbSet<Report> Reports { get; set; }

        public async Task<List<Conversation>> MyConversations(string userId)
        {
            var personalRelations = await this.PrivateConversations
                .AsNoTracking()
                .Where(t => t.RequesterId == userId || t.TargetId == userId)
                .Include(t => t.RequestUser)
                .Include(t => t.TargetUser)
                .Include(t => t.Messages)
                .ToListAsync();
            var groups = await this.UserGroupRelations
                .AsNoTracking()
                .Where(t => t.UserId == userId)
                .Include(t => t.Group.Messages)
                .ToListAsync();
            var myConversations = new List<Conversation>();
            myConversations.AddRange(personalRelations);
            myConversations.AddRange(groups.Select(t => t.Group));
            return myConversations;
        }

        public async Task<UserGroupRelation> GetRelationFromGroup(string userId, int groupId)
        {
            return await this.UserGroupRelations
                .SingleOrDefaultAsync(t => t.UserId == userId && t.GroupId == groupId);
        }

        public async Task<bool> VerifyJoined(string userId, Conversation target)
        {
            if (target == null)
            {
                return false;
            }
            else if (target.Discriminator == nameof(GroupConversation))
            {
                var relation = await this
                    .UserGroupRelations
                    .SingleOrDefaultAsync(t => t.UserId == userId && t.GroupId == target.Id);
                if (relation == null)
                    return false;
            }
            else if (target.Discriminator == nameof(PrivateConversation))
            {
                var privateConversation = target as PrivateConversation;
                if (privateConversation.RequesterId != userId && privateConversation.TargetId != userId)
                    return false;
            }
            return true;
        }

        public async Task<PrivateConversation> FindConversationAsync(string userId1, string userId2)
        {
            var relation = await this.PrivateConversations.SingleOrDefaultAsync(t => t.RequesterId == userId1 && t.TargetId == userId2);
            var belation = await this.PrivateConversations.SingleOrDefaultAsync(t => t.RequesterId == userId2 && t.TargetId == userId1);
            if (relation != null) return relation;
            else if (belation != null) return belation;
            else return null;
        }

        public async Task<bool> AreFriends(string userId1, string userId2)
        {
            var conversation = await FindConversationAsync(userId1, userId2);
            return conversation != null;
        }

        public async Task RemoveFriend(string userId1, string userId2)
        {
            var relation = await this.PrivateConversations.SingleOrDefaultAsync(t => t.RequesterId == userId1 && t.TargetId == userId2);
            var belation = await this.PrivateConversations.SingleOrDefaultAsync(t => t.RequesterId == userId2 && t.TargetId == userId1);
            if (relation != null) this.PrivateConversations.Remove(relation);
            if (belation != null) this.PrivateConversations.Remove(belation);
        }

        public async Task<GroupConversation> CreateGroup(string groupName, string creatorId)
        {
            var newGroup = new GroupConversation
            {
                GroupName = groupName,
                GroupImageKey = Values.DefaultGroupImageId,
                AESKey = Guid.NewGuid().ToString("N"),
                OwnerId = creatorId
            };
            this.GroupConversations.Add(newGroup);
            await this.SaveChangesAsync();
            return newGroup;
        }

        public void AddFriend(string userId1, string userId2)
        {
            this.PrivateConversations.Add(new PrivateConversation
            {
                RequesterId = userId1,
                TargetId = userId2,
                AESKey = Guid.NewGuid().ToString("N")
            });
        }
    }
}
