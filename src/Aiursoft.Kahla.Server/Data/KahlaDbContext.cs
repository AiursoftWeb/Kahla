using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Models.Conversations;
using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.SDK.ModelsOBS;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Data
{
    public class KahlaDbContext(DbContextOptions<KahlaDbContext> options) : IdentityDbContext<KahlaUser>(options)
    {
        public DbSet<Message> Messages { get; set; }
        
        [Obsolete]
        public DbSet<Request> Requests { get; set; }
        [Obsolete]
        public DbSet<PrivateConversation> PrivateConversations { get; set; }
        [Obsolete]
        public DbSet<GroupConversation> GroupConversations { get; set; }
        [Obsolete]
        public DbSet<UserGroupRelation> UserGroupRelations { get; set; }
        [Obsolete]
        public DbSet<Conversation> Conversations { get; set; }
        
        public DbSet<ChatThread> ChatThreads { get; set; }
        public DbSet<ContactRecord> ContactRecords { get; set; }
        public DbSet<BlockRecord> BlockRecords { get; set; }
        public DbSet<UserThreadRelation> UserThreadRelations { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Device> Devices { get; set; }

        private IQueryable<KahlaThreadMappedJoinedView> MapThreadsQueryToJoinedView(IQueryable<ChatThread> threads, string userId)
        {
            return threads
                .AsNoTracking()
                .Select(t => new KahlaThreadMappedJoinedView
                {
                    Id = t.Id,
                    Name = t.Name,
                    ImagePath = t.IconFilePath,
                    OwnerId = t.OwnerRelation.UserId,
                    AllowDirectJoinWithoutInvitation = t.AllowDirectJoinWithoutInvitation,
                    UnReadAmount = t.Messages.Count(m => m.SendTime > t.Members.SingleOrDefault(u => u.UserId == userId)!.ReadTimeStamp),
                    LatestMessage = t.Messages
                        .OrderByDescending(p => p.SendTime)
                        .FirstOrDefault(),
                    LatestMessageSender = t.Messages.Any() ? t.Messages
                        .OrderByDescending(p => p.SendTime)
                        .Select(m => m.Sender)
                        .FirstOrDefault() : null,
                    Muted = t.Members.SingleOrDefault(u => u.UserId == userId)!.Muted,
                    TopTenMembers = t.Members
                        .OrderBy(p => p.JoinTime)
                        .Select(p => p.User)
                        .Take(10),
                });
        }
        
        public IQueryable<KahlaThreadMappedJoinedView> QueryJoinedThreads(string userId)
        {
            var query = ChatThreads
                .AsNoTracking()
                .Where(t => t.Members.Any(p => p.UserId == userId));
            return MapThreadsQueryToJoinedView(query, userId);
        }
        
        public IQueryable<KahlaThreadMappedJoinedView> QueryCommonThreads(string userId, string targetUserId)
        {
            var query = ChatThreads
                .AsNoTracking()
                .Where(t => t.Members.Any(p => p.UserId == userId))
                .Where(t => t.Members.Any(p => p.UserId == targetUserId));
            return MapThreadsQueryToJoinedView(query, userId);
        }
    }
}
