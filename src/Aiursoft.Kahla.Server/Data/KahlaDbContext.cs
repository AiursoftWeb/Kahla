using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Models.Conversations;
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
    }
}
