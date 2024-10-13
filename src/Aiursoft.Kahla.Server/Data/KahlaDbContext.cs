using Aiursoft.Kahla.SDK.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Data
{
    public class KahlaDbContext(DbContextOptions<KahlaDbContext> options) : IdentityDbContext<KahlaUser>(options)
    {
        public DbSet<Message> Messages { get; set; }
        public DbSet<ChatThread> ChatThreads { get; set; }
        public DbSet<ContactRecord> ContactRecords { get; set; }
        public DbSet<BlockRecord> BlockRecords { get; set; }
        public DbSet<UserThreadRelation> UserThreadRelations { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Device> Devices { get; set; }
    }
}
