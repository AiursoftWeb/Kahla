using Aiursoft.DbTools;
using Aiursoft.Kahla.Server.Data;

namespace Aiursoft.Kahla.Server;

public abstract class Program
{
    public static async Task Main(string[] args)
    {
        var app = await WebTools.Extends.AppAsync<Startup>(args);
        await app.UpdateDbAsync<KahlaDbContext>(UpdateMode.MigrateThenUse);
        await app.Services.GetRequiredService<QuickMessageAccess>().LoadAsync();
        await app.Services.GetRequiredService<InfluxDbClient>().EnsureDatabaseCreatedAsync();
        await app.RunAsync();
    }
}
