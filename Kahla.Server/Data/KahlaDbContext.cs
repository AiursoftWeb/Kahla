using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Kahla.Server.Models;
using Microsoft.Extensions.Configuration;

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

        public async Task<List<string>> MyPersonalFriendsId(string userId)
        {
            var personalRelations = await PrivateConversations
                .AsNoTracking()
                .Where(t => t.RequesterId == userId || t.TargetId == userId)
                .Select(t => userId == t.RequesterId ? t.TargetId : t.RequesterId)
                .ToListAsync();
            return personalRelations;
        }

        public async Task<List<Conversation>> MyConversations(string userId)
        {
            var personalRelations = await PrivateConversations
                .AsNoTracking()
                .Where(t => t.RequesterId == userId || t.TargetId == userId)
                .Include(t => t.RequestUser)
                .Include(t => t.TargetUser)
                .Include(t => t.Messages)
                .ThenInclude(t => t.Ats)
                .ToListAsync();
            var groups = await GroupConversations
                .AsNoTracking()
                .Where(t => t.Users.Any(p => p.UserId == userId))
                .Include(t => t.Messages)
                .ThenInclude(t => t.Ats)
                .Include(t => t.Users)
                .ThenInclude(t => t.User)
                .ToListAsync();
            var myConversations = new List<Conversation>();
            myConversations.AddRange(personalRelations);
            myConversations.AddRange(groups);
            return myConversations;
        }

        public async Task<UserGroupRelation> GetRelationFromGroup(string userId, int groupId)
        {
            return await UserGroupRelations
                .SingleOrDefaultAsync(t => t.UserId == userId && t.GroupId == groupId);
        }

        public async Task<bool> VerifyJoined(string userId, Conversation target)
        {
            if (target == null)
            {
                return false;
            }
            switch (target.Discriminator)
            {
                case nameof(GroupConversation):
                    {
                        var relation = await UserGroupRelations
                            .SingleOrDefaultAsync(t => t.UserId == userId && t.GroupId == target.Id);
                        if (relation == null)
                            return false;
                        break;
                    }
                case nameof(PrivateConversation):
                    {
                        var privateConversation = target as PrivateConversation;
                        if (privateConversation?.RequesterId != userId && privateConversation?.TargetId != userId)
                            return false;
                        break;
                    }
            }
            return true;
        }

        public async Task<PrivateConversation> FindConversationAsync(string userId1, string userId2)
        {
            var relation = await PrivateConversations.SingleOrDefaultAsync(t => t.RequesterId == userId1 && t.TargetId == userId2);
            if (relation != null) return relation;
            var belation = await PrivateConversations.SingleOrDefaultAsync(t => t.RequesterId == userId2 && t.TargetId == userId1);
            return belation;
        }

        public async Task<bool> AreFriends(string userId1, string userId2)
        {
            var conversation = await FindConversationAsync(userId1, userId2);
            return conversation != null;
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
                GroupImageKey = Convert.ToInt32(_configuration["GroupImageKey"]),
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
