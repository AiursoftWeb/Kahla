using Microsoft.Extensions.Hosting;
using static Aiursoft.WebTools.Extends;

namespace Kahla.Home
{
    public class Program
    {
        public static void Main(string[] args)
        {
            App<Startup>(args).Run();
        }

        // For EF
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return BareApp<Startup>(args);
        }
    }
}
