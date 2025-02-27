using Aiursoft.DbTools;
using Aiursoft.DbTools.Sqlite;
using Aiursoft.Kahla.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Aiursoft.Kahla.Sqlite;

public class SqliteSupportedDb(bool allowCache, bool splitQuery) : SupportedDatabaseType<KahlaRelationalDbContext>
{
    public override string DbType => "Sqlite";

    public override IServiceCollection RegisterFunction(IServiceCollection services, string connectionString)
    {
        return services.AddAiurSqliteWithCache<SqliteContext>(
            connectionString,
            splitQuery: splitQuery,
            allowCache: allowCache);
    }

    public override KahlaRelationalDbContext ContextResolver(IServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<SqliteContext>();
    }
}