using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;
using Kahla.Server.Data;
using Aiursoft.Pylon;

namespace Kahla.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args)
                .MigrateDbContext<KahlaDbContext>()
                .Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            var host = WebHost.CreateDefaultBuilder(args)
                 .UseApplicationInsights()
                 .UseStartup<Startup>()
                 .Build();

            return host;
        }
    }
}
