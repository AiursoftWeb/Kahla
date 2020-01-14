using Aiursoft.XelNaga.Interfaces;
using System;

namespace Kahla.SDK.Services
{
    public class BotLogger : ISingletonDependency
    {
        private static object _obj = new object();
        public string ReadLine(string ask)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(ask);
            return Console.ReadLine();
        }

        public void LogSuccess(string success)
        {
            lock (_obj)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(success);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        public void LogInfo(string info)
        {
            lock (_obj)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(info);
            }
        }

        public void LogWarning(string warning)
        {
            lock (_obj)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(warning);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        public void LogDanger(string danger)
        {
            lock (_obj)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(danger);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        public void LogVerbose(string warning)
        {
            lock (_obj)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(warning);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }
}
