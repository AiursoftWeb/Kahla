using Microsoft.Extensions.DependencyInjection;

namespace Kahla.Bot
{
    public class Program
    {
        static void Main(string[] args)
        {
            StartUp.ConfigureServices()
                .ServiceProvider
                .GetService<StartUp>()
                .Start()
                .Wait();
        }
    }
}
