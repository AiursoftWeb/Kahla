using Aiursoft.Kahla.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Sqlite;

public class SqliteContext(DbContextOptions<SqliteContext> options) : KahlaRelationalDbContext(options)
{
    public override Task<bool> CanConnectAsync()
    {
        return Task.FromResult(true);
    }
}
