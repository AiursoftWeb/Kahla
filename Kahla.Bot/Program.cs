using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.Bot
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            var asService = false;
            if (args.Any() && args[0].Trim() == "as-service")
            {
                asService = true;
            }

            await StartUp.ConfigureServices()
                .GetService<StartUp>()
                .Bot
                .Start(enableCommander: !asService);
        }
    }
}
