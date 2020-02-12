using Aiursoft.Scanner.Interfaces;
using System;

namespace Kahla.SDK.Services
{
    public class BotLogger : ISingletonDependency
    {
        private static object _obj = new object();
        public string ReadLine(string ask)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("\n" + ask);
            return Console.ReadLine();
        }

        public void AppendResult(bool success, int tabs = 1)
        {
            Console.ForegroundColor = ConsoleColor.White;
            for (int i = 0; i < tabs; i++)
            {
                Console.Write("\t");
            }
            lock (_obj)
            {
                Console.Write("[");
                if (success)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("  OK  ");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(" FAIL ");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                Console.Write("]");
            }
        }

        public void LogSuccess(string success)
        {
            lock (_obj)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(success + "\n");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        public void LogInfo(string info)
        {
            lock (_obj)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("\n" + info);
            }
        }

        public void LogWarning(string warning)
        {
            lock (_obj)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("\n" + warning);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        public void LogDanger(string danger)
        {
            lock (_obj)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("\n" + danger);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        public void LogVerbose(string warning)
        {
            lock (_obj)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("\n" + warning);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }
}
