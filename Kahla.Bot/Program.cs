﻿using Kahla.Bot.Bots;
using Kahla.SDK.Abstract;

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
