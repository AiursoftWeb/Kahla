using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Kahla.Home
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
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
