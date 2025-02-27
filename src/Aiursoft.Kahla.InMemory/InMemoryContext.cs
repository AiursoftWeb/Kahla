using Aiursoft.Kahla.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.InMemory;

public class InMemoryContext(DbContextOptions<InMemoryContext> options) : KahlaRelationalDbContext(options)
{
    public override Task MigrateAsync(CancellationToken cancellationToken)
    {
        return Database.EnsureCreatedAsync(cancellationToken);
    }

    public override Task<bool> CanConnectAsync()
    {
        return Task.FromResult(true);
    }
}
