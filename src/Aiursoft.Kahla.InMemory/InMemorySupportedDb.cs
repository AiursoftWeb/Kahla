using Aiursoft.DbTools;
using Aiursoft.DbTools.InMemory;
using Aiursoft.Kahla.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Aiursoft.Kahla.InMemory;

public class InMemorySupportedDb : SupportedDatabaseType<KahlaRelationalDbContext>
{
    public override string DbType => "InMemory";

    public override IServiceCollection RegisterFunction(IServiceCollection services, string connectionString)
    {
        return services.AddAiurInMemoryDb<InMemoryContext>();
    }

    public override KahlaRelationalDbContext ContextResolver(IServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<InMemoryContext>();
    }
}