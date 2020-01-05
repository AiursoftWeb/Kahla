using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using Kahla.SDK.Services;

namespace Kahla.Bot
{
    public class Program
    {
        static void Main(string[] args)
        {
            if (args.Any() && args[0].Trim() == "version")
            {
                Console.WriteLine(VersionService.SDKVersion());
                return;
            }

            StartUp.ConfigureServices()
                .ServiceProvider
                .GetService<StartUp>()
                .Start()
                .Wait();
        }
    }
}
