using Aiursoft.Archon.SDK.Services;
using Aiursoft.Probe.SDK;
using Aiursoft.SDK;
using Kahla.Server.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Kahla.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args)
                .Build()
                .MigrateDbContext<KahlaDbContext>()
                .InitSite<AppsContainer>(c => c["UserIconsSiteName"], a => a.AccessToken())
                .InitSite<AppsContainer>(c => c["UserFilesSiteName"], a => a.AccessToken())
                .Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
        }
    }
}
