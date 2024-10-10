using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Models.Conversations;
using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.SDK.ModelsOBS;
using Aiursoft.Kahla.SDK.ModelsOBS.ApiViewModels;
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
        public DbSet<UserThreadRelation> UserThreadRelations { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Device> Devices { get; set; }

        private IQueryable<KahlaThreadMappedJoinedView> Map(IQueryable<ChatThread> threads, string userId)
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
            return Map(query, userId);
            // return ChatThreads
            //     .AsNoTracking()
            //     .Where(t => t.Members.Any(p => p.UserId == userId))
            //     .Select(t => new KahlaThreadMappedJoinedView
            //     {
            //         Id = t.Id,
            //         Name = t.Name,
            //         ImagePath = t.IconFilePath,
            //         OwnerId = t.OwnerRelation.UserId,
            //         AllowDirectJoinWithoutInvitation = t.AllowDirectJoinWithoutInvitation,
            //         UnReadAmount = t.Messages.Count(m => m.SendTime > t.Members.SingleOrDefault(u => u.UserId == userId)!.ReadTimeStamp),
            //         LatestMessage = t.Messages.OrderByDescending(p => p.SendTime).FirstOrDefault(),
            //         Muted = t.Members.SingleOrDefault(u => u.UserId == userId)!.Muted,
            //         TopTenMembers = t.Members
            //             .OrderBy(p => p.JoinTime)
            //             .Select(p => p.User)
            //             .Take(10)
            //     });
        }
        
        public IQueryable<KahlaThreadMappedJoinedView> QueryCommonThreads(string userId, string targetUserId)
        {
            var query = ChatThreads
                .AsNoTracking()
                .Where(t => t.Members.Any(p => p.UserId == userId))
                .Where(t => t.Members.Any(p => p.UserId == targetUserId));
            return Map(query, userId);
            // return ChatThreads
            //     .AsNoTracking()
            //     .Where(t => t.Members.Any(p => p.UserId == userId))
            //     .Where(t => t.Members.Any(p => p.UserId == targetUserId))
            //     .Select(t => new KahlaThreadMappedJoinedView
            //     {
            //         Id = t.Id,
            //         Name = t.Name,
            //         ImagePath = t.IconFilePath,
            //         OwnerId = t.OwnerRelation.UserId,
            //         AllowDirectJoinWithoutInvitation = t.AllowDirectJoinWithoutInvitation,
            //         UnReadAmount = t.Messages.Count(m => m.SendTime > t.Members.SingleOrDefault(u => u.UserId == userId)!.ReadTimeStamp),
            //         LatestMessage = t.Messages.OrderByDescending(p => p.SendTime).FirstOrDefault(),
            //         Muted = t.Members.SingleOrDefault(u => u.UserId == userId)!.Muted,
            //         TopTenMembers = t.Members
            //             .OrderBy(p => p.JoinTime)
            //             .Select(p => p.User)
            //             .Take(10)
            //     });
        }

        #nullable disable
        [Obsolete]
        public IQueryable<ContactInfo> MyContacts(string userId)
        {
            return Conversations
                .AsNoTracking()
                .Where(t => !(t is PrivateConversation) || ((PrivateConversation)t).RequesterId == userId || ((PrivateConversation)t).TargetId == userId)
                .Where(t => !(t is GroupConversation) || ((GroupConversation)t).Users.Any(p => p.UserId == userId))
                .Select(t => new ContactInfo
                {
                    ConversationId = t.Id,
                    Discriminator = t.Discriminator!,

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

                    LatestMessage = t.Messages.OrderByDescending(p => p.SendTime).FirstOrDefault(),
                    Sender = t.Messages.Any() ? t.Messages.OrderByDescending(p => p.SendTime).Select(m => m.Sender).FirstOrDefault() : null,

                    Muted = t is GroupConversation && ((GroupConversation)t).Users.SingleOrDefault(u => u.UserId == userId).Muted
                })
                .OrderByDescending(t => t.LatestMessage == null ? DateTime.MinValue : t.LatestMessage.SendTime);
        }
        #nullable enable

        [Obsolete]
        public async Task<UserGroupRelation?> GetRelationFromGroup(string userId, int groupId)
        {
            return await UserGroupRelations
                .SingleOrDefaultAsync(t => t.UserId == userId && t.GroupId == groupId);
        }
        
        [Obsolete]
        public Task<PrivateConversation?> FindConversationAsync(string userId1, string userId2)
        {
            return PrivateConversations.Where(t =>
                    (t.RequesterId == userId1 && t.TargetId == userId2) ||
                    (t.RequesterId == userId2 && t.TargetId == userId1)).FirstOrDefaultAsync();
        }

        [Obsolete]
        public async Task<bool> AreFriends(string userId1, string userId2)
        {
            return await FindConversationAsync(userId1, userId2) != null;
        }

        [Obsolete]

        public async Task<int> RemoveFriend(string userId1, string userId2)
        {
            var relation = await PrivateConversations.SingleOrDefaultAsync(t => t.RequesterId == userId1 && t.TargetId == userId2);
            var belation = await PrivateConversations.SingleOrDefaultAsync(t => t.RequesterId == userId2 && t.TargetId == userId1);
            if (relation != null)
            {
                PrivateConversations.Remove(relation);
                return relation.Id;
            }
            if (belation != null)
            {
                PrivateConversations.Remove(belation);
                return belation.Id;
            }
            return -1;
        }




    }
}
