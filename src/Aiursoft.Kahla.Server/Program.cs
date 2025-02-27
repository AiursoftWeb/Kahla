using System.Diagnostics.CodeAnalysis;
using Aiursoft.DbTools;
using Aiursoft.Kahla.Entities;
using Aiursoft.Kahla.Server.Data;
using Aiursoft.WebTools;

namespace Aiursoft.Kahla.Server;

[ExcludeFromCodeCoverage]
public abstract class Program
{
    public static async Task Main(string[] args)
    {
        var app = await Extends.AppAsync<Startup>(args);
        await app.UpdateDbAsync<KahlaRelationalDbContext>();
        await app.Services.GetRequiredService<QuickMessageAccess>().LoadAsync();
        await app.RunAsync();
    }
}
