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
            await (await (await App<Startup>(args)
                .Update<KahlaDbContext>()
                .InitSite<AppsContainer>(c => c["UserIconsSiteName"], a => a.GetAccessTokenAsync()))
                .InitSite<AppsContainer>(c => c["UserFilesSiteName"], a => a.GetAccessTokenAsync()))
                .RunAsync();
        }

        // For EF
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return BareApp<Startup>(args);
        }
    }
}
