using Kahla.SDK.Abstract;
using System.Threading.Tasks;

namespace Kahla.SDK.CommandHandlers
{
    [CommandHandler("req")]
    public class ReqCommandHandler : CommandHandlerBase
    {
        public ReqCommandHandler(BotCommander botCommander) : base(botCommander)
        {
        }

        public async override Task Execute(string command)
        {
            await Task.Delay(0);
            foreach (var request in _botCommander._botHost._bot.EventSyncer.Requests)
            {
                _botCommander._botLogger.LogInfo($"Name:\t{request.Creator.NickName}");
                _botCommander._botLogger.LogInfo($"Time:\t{request.CreateTime}");
                if (request.Completed)
                {
                    _botCommander._botLogger.LogSuccess($"\tCompleted.");
                }
                else
                {
                    _botCommander._botLogger.LogWarning($"\tPending.");
                }

                _botCommander._botLogger.LogInfo($"\n");
            }
        }
    }
}
