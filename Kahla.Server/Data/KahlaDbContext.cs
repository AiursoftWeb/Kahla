using Kahla.Server.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.Server.Data
{
    public class KahlaDbContext : IdentityDbContext<KahlaUser>
    {
        private readonly IConfiguration _configuration;
        public KahlaDbContext(
            DbContextOptions<KahlaDbContext> options,
            IConfiguration configuration) : base(options)
        {
            _configuration = configuration;
        }

        public DbSet<Message> Messages { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<PrivateConversation> PrivateConversations { get; set; }
        public DbSet<GroupConversation> GroupConversations { get; set; }
        public DbSet<UserGroupRelation> UserGroupRelations { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<At> Ats { get; set; }

        public IEnumerable<Conversation> MyConversations(string userId)
        {
            return Conversations
                .AsNoTracking()
                .Include(t => (t as PrivateConversation).TargetUser)
                .Include(t => (t as PrivateConversation).RequestUser)
                .Include(t => (t as GroupConversation).UserRelations)
                .ThenInclude(t => t.User)
                .Include(t => (t as GroupConversation).Owner)
                .Include(t => t.Messages)
                .ThenInclude(t => t.Ats)
                .AsEnumerable()
#warning https://github.com/aspnet/EntityFrameworkCore/issues/17546
#warning https://github.com/aspnet/EntityFrameworkCore/issues/17475
                .Where(t => t.HasUser(userId));
        }

        public async Task<UserGroupRelation> GetRelationFromGroup(string userId, int groupId)
        {
            return await UserGroupRelations
                .SingleOrDefaultAsync(t => t.UserId == userId && t.GroupId == groupId);
        }

        public Task<PrivateConversation> FindConversationAsync(string userId1, string userId2)
        {
            return PrivateConversations.Where(t =>
                    (t.RequesterId == userId1 && t.TargetId == userId2) ||
                    (t.RequesterId == userId2 && t.TargetId == userId1)).FirstOrDefaultAsync();
        }

        public async Task<bool> AreFriends(string userId1, string userId2)
        {
            return await FindConversationAsync(userId1, userId2) != null;
        }

        public async Task RemoveFriend(string userId1, string userId2)
        {
            var relation = await PrivateConversations.SingleOrDefaultAsync(t => t.RequesterId == userId1 && t.TargetId == userId2);
            var belation = await PrivateConversations.SingleOrDefaultAsync(t => t.RequesterId == userId2 && t.TargetId == userId1);
            if (relation != null) PrivateConversations.Remove(relation);
            if (belation != null) PrivateConversations.Remove(belation);
        }

        public async Task<GroupConversation> CreateGroup(string groupName, string creatorId, string joinPassword)
        {
            var newGroup = new GroupConversation
            {
                GroupName = groupName,
                GroupImagePath = _configuration["GroupImagePath"],
                AESKey = Guid.NewGuid().ToString("N"),
                OwnerId = creatorId,
                JoinPassword = joinPassword ?? string.Empty
            };
            GroupConversations.Add(newGroup);
            await SaveChangesAsync();
            return newGroup;
        }

        public void AddFriend(string userId1, string userId2)
        {
            PrivateConversations.Add(new PrivateConversation
            {
                RequesterId = userId1,
                TargetId = userId2,
                AESKey = Guid.NewGuid().ToString("N")
            });
        }
    }
}
