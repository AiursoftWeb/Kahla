using Aiursoft.DbTools;
using Aiursoft.Directory.SDK.Services;
using Aiursoft.Probe.SDK;
using Kahla.Server.Data;
using static Aiursoft.WebTools.Extends;

namespace Kahla.Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var app = await Aiursoft.WebTools.Extends.AppAsync<Startup>(args);
            await app.UpdateDbAsync<KahlaDbContext>(UpdateMode.MigrateThenUse);
            await app.InitSiteAsync<DirectoryAppTokenService>(c => c["UserIconsSiteName"],
                a => a.GetAccessTokenAsync());
            await app.InitSiteAsync<DirectoryAppTokenService>(c => c["UserFilesSiteName"],
                a => a.GetAccessTokenAsync());
            await app.RunAsync();
        }
    }
}
