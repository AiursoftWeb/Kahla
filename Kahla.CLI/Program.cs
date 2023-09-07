using Kahla.SDK.Abstract;

namespace Kahla.CLI
{
    public class EmptyBot : BotBase { }

    public class Program
    {
        public async static Task Main(string[] args)
        {
            await CreateBotBuilder()
                .Build<EmptyBot>()
                .Run(
                    enableCommander: true,
                    autoReconnectMax: 10);
        }

        public static BotBuilder CreateBotBuilder()
        {
            return new BotBuilder();
        }
    }
}
