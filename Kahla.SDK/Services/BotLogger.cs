using Aiursoft.XelNaga.Interfaces;
using System;

namespace Kahla.Bot.Services
{
    public class BotLogger : ISingletonDependency
    {
        public void LogSuccess(string success)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(success);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public void LogInfo(string info)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(info);
        }

        public void LogWarning(string warning)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(warning);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public void LogDanger(string danger)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(danger);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public void LogVerbose(string warning)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(warning);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
