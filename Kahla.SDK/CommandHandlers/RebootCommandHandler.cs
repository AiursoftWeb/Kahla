//using Kahla.SDK.Abstract;
//using System;
//using System.Threading.Tasks;

//namespace Kahla.SDK.CommandHandlers
//{
//    public class RebootCommandHandler: CommandHandlerBase
//    {
//        public RebootCommandHandler() 
//        {
//        }

//        public override bool CanHandle(string command)
//        {
//            return command.StartsWith("reboot");
//        }

//        public async override Task Execute(string command)
//        {
//            await Task.Delay(0);
//            Console.Clear();
//            var _ = _botCommander.BotHost.Connect().ConfigureAwait(false);
//        }
//    }
//}
