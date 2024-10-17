using Aiursoft.Kahla.SDK.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Data
{
    public class KahlaDbContext(DbContextOptions<KahlaDbContext> options) : IdentityDbContext<KahlaUser>(options)
    {
        public DbSet<ChatThread> ChatThreads => Set<ChatThread>();
        public DbSet<ContactRecord> ContactRecords => Set<ContactRecord>();
        public DbSet<BlockRecord> BlockRecords => Set<BlockRecord>();
        public DbSet<UserThreadRelation> UserThreadRelations => Set<UserThreadRelation>();
        public DbSet<Report> Reports => Set<Report>();
        public DbSet<Device> Devices => Set<Device>();
    }
}
