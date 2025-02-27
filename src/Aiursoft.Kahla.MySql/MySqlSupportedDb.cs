using Aiursoft.DbTools;
using Aiursoft.DbTools.MySql;
using Aiursoft.Kahla.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Aiursoft.Kahla.MySql;

public class MySqlSupportedDb(bool allowCache, bool splitQuery) : SupportedDatabaseType<KahlaRelationalDbContext>
{
    public override string DbType => "MySql";

    public override IServiceCollection RegisterFunction(IServiceCollection services, string connectionString)
    {
        return services.AddAiurMySqlWithCache<MySqlContext>(
            connectionString,
            splitQuery: splitQuery,
            allowCache: allowCache);
    }

    public override KahlaRelationalDbContext ContextResolver(IServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<MySqlContext>();
    }
}