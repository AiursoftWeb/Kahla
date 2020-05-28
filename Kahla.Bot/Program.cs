using Kahla.Bot.Bots;
using Kahla.SDK.Abstract;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.Bot
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            await CreateBotBuilder()
#warning select Bot
                .Build<EchoBot>()
                .Run(args.FirstOrDefault() != "as-service");
        }

        public static BotBuilder CreateBotBuilder()
        {
            return new BotBuilder()
                .UseStartUp<StartUp>();
        }
    }
}
