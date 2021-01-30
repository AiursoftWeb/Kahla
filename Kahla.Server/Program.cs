using Aiursoft.Archon.SDK.Services;
using Aiursoft.Probe.SDK;
using Aiursoft.SDK;
using Kahla.Server.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using static Aiursoft.WebTools.Extends;

namespace Kahla.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            App<Startup>(args)
                .Update<KahlaDbContext>()
                .InitSite<AppsContainer>(c => c["UserIconsSiteName"], a => a.AccessToken())
                .InitSite<AppsContainer>(c => c["UserFilesSiteName"], a => a.AccessToken())
                .Run();
        }

        // For EF
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return BareApp<Startup>(args);
        }
    }
}
