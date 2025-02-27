using Aiursoft.DbTools;
using Aiursoft.Kahla.Entities.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Entities
{
    public abstract class KahlaRelationalDbContext(DbContextOptions options) : IdentityDbContext<KahlaUser>(options), ICanMigrate
    {
        public DbSet<ChatThread> ChatThreads => Set<ChatThread>();
        public DbSet<ContactRecord> ContactRecords => Set<ContactRecord>();
        public DbSet<BlockRecord> BlockRecords => Set<BlockRecord>();
        public DbSet<UserThreadRelation> UserThreadRelations => Set<UserThreadRelation>();
        public DbSet<Report> Reports => Set<Report>();
        public DbSet<Device> Devices => Set<Device>();

        public virtual  Task MigrateAsync(CancellationToken cancellationToken) =>
            Database.MigrateAsync(cancellationToken);

        public virtual  Task<bool> CanConnectAsync() =>
            Database.CanConnectAsync();
    }
}
