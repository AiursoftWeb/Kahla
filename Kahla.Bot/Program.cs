using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.Bot
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            var command = true;
            if (args.Any() && args[0].Trim() == "no-command")
            {
                command = false;
            }

            await StartUp.ConfigureServices()
                .GetService<StartUp>()
                .Bot
                .Start(command);
        }
    }
}
