using Kahla.Bot.Bots;
using Kahla.SDK.Abstract;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.Bot
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            await CreateBotBuilder()
                .Build<EchoBot>()
                .Run(
                    enableCommander: args.FirstOrDefault() != "as-service",
                    autoReconnectMax: int.MaxValue);
        }

        public static BotBuilder CreateBotBuilder()
        {
            return new BotBuilder()
                .UseStartUp<StartUp>();
        }
    }
}
