using System;

namespace Kahla.SDK.Abstract
{
    public class CommandHandlerAttribute : Attribute
    {
        public string Command { get; }
        public CommandHandlerAttribute(string command)
        {
            Command = command;
        }
    }
}
