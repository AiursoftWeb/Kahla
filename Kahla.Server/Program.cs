using System.Threading.Tasks;
using Aiursoft.Directory.SDK.Services;
using Aiursoft.Probe.SDK;
using Aiursoft.SDK;
using Kahla.Server.Data;
using Microsoft.Extensions.Hosting;
using static Aiursoft.WebTools.Extends;

namespace Kahla.Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var app = App<Startup>(args);
            await app.UpdateDbAsync<KahlaDbContext>();
            await app.InitSiteAsync<DirectoryAppTokenService>(c => c["UserIconsSiteName"],
                a => a.GetAccessTokenAsync());
            await app.InitSiteAsync<DirectoryAppTokenService>(c => c["UserFilesSiteName"],
                a => a.GetAccessTokenAsync());
            await app.RunAsync();
        }
    }
}
