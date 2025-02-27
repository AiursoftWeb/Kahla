using Aiursoft.Kahla.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.InMemory;

public class InMemoryContext(DbContextOptions<InMemoryContext> options) : KahlaRelationalDbContext(options)
{
    public override async Task MigrateAsync(CancellationToken cancellationToken)
    {
        await Database.EnsureDeletedAsync(cancellationToken);
        await Database.EnsureCreatedAsync(cancellationToken);
    }

    public override Task<bool> CanConnectAsync()
    {
        return Task.FromResult(true);
    }
}
