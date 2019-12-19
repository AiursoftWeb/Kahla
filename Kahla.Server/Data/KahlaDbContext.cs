using Kahla.SDK.Models;
using Kahla.SDK.Models.ApiViewModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
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

        public IQueryable<ContactInfo> MyContacts(string userId)
        {
            return Conversations
                .AsNoTracking()
                .Where(t => !(t is PrivateConversation) || ((PrivateConversation)t).RequesterId == userId || ((PrivateConversation)t).TargetId == userId)
                .Where(t => !(t is GroupConversation) || ((GroupConversation)t).Users.Any(p => p.UserId == userId))
                .Select(t => new ContactInfo
                {
                    ConversationId = t.Id,
                    Discriminator = t.Discriminator,

                    DisplayName = (t is PrivateConversation) ?
                        (userId == ((PrivateConversation)t).RequesterId ? ((PrivateConversation)t).TargetUser.NickName : ((PrivateConversation)t).RequestUser.NickName) :
                        ((GroupConversation)t).GroupName,
                    DisplayImagePath = (t is PrivateConversation) ?
                        (userId == ((PrivateConversation)t).RequesterId ? ((PrivateConversation)t).TargetUser.IconFilePath : ((PrivateConversation)t).RequestUser.IconFilePath) :
                        ((GroupConversation)t).GroupImagePath,
                    UserId = (t is PrivateConversation) ?
                        (userId == ((PrivateConversation)t).RequesterId ? ((PrivateConversation)t).TargetId : ((PrivateConversation)t).RequesterId) :
                        ((GroupConversation)t).OwnerId,
                    UnReadAmount =
                        (t is GroupConversation) ?
                        t.Messages.Count(m => m.SendTime > ((GroupConversation)t).Users.SingleOrDefault(u => u.UserId == userId).ReadTimeStamp) :
                        t.Messages.Count(p => !p.Read && p.SenderId != userId),

                    LatestMessage = t.Messages.OrderByDescending(p => p.SendTime).Select(m => m.Content).FirstOrDefault(),
                    LatestMessageTime = t.Messages.Max(m => m.SendTime),

                    Muted = (t is GroupConversation) ?
                        ((GroupConversation)t).Users.SingleOrDefault(u => u.UserId == userId).Muted : false,
                    AesKey = t.AESKey,
                    SomeoneAtMe = (t is GroupConversation) ? t.Messages
                        .Where(m => m.SendTime > ((GroupConversation)t).Users.SingleOrDefault(u => u.UserId == userId).ReadTimeStamp)
                        .Any(p => p.Ats.Any(k => k.TargetUserId == userId)) : false,
                    Online = (t is PrivateConversation) ?
                        (userId == ((PrivateConversation)t).RequesterId ? ((PrivateConversation)t).TargetUser.IsOnline : ((PrivateConversation)t).RequestUser.IsOnline) :
                        (bool?)null,
                })
                .OrderByDescending(t => t.SomeoneAtMe)
                .ThenByDescending(t => t.LatestMessageTime);
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

        public async Task<DateTime> SetLastRead(Conversation conversation, string userId)
        {
            if (conversation is PrivateConversation)
            {
                var query = Messages
                    .Where(t => t.ConversationId == conversation.Id)
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
            else if (conversation is GroupConversation)
            {
                var relation = await UserGroupRelations
                        .SingleOrDefaultAsync(t => t.UserId == userId && t.GroupId == conversation.Id);
                try
                {
                    return relation.ReadTimeStamp;
                }
                finally
                {
                    relation.ReadTimeStamp = DateTime.UtcNow;
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
