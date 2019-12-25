using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace Kahla.Bot
{
    public class Program
    {
        public static string GetSDKVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString().Split('.');
            return $"{version[0]}.{version[1]}.{version[2]}";
        }

        static void Main(string[] args)
        {
            if (args.Any() && args[0].Trim() == "version")
            {
                Console.WriteLine(GetSDKVersion());
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
