using Kahla.SDK.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.Bot
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            if (args.Any() && args[0].Trim() == "version")
            {
                Console.WriteLine(VersionService.SDKVersion());
                return;
            }

            await StartUp.ConfigureServices()
                .GetService<StartUp>()
                .Bot
                .Start();
        }
    }
}
