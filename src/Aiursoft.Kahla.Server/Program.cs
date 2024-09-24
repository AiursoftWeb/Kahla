using Aiursoft.DbTools;
using Aiursoft.Kahla.Server.Data;

namespace Aiursoft.Kahla.Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var app = await WebTools.Extends.AppAsync<Startup>(args);
            await app.UpdateDbAsync<KahlaDbContext>(UpdateMode.MigrateThenUse);
            await app.RunAsync();
        }
    }
}
