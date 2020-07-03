using Kahla.SDK.Abstract;
using Kahla.SDK.Data;
using Kahla.SDK.Services;
using System.Threading.Tasks;

namespace Kahla.SDK.CommandHandlers
{
    public class ReqCommandHandler<T> : ICommandHandler<T> where T : BotBase
    {
        private readonly BotLogger _botLogger;
        private readonly EventSyncer<T> _eventSyncer;

        public ReqCommandHandler(
            BotLogger botLogger,
            EventSyncer<T> eventSyncer)
        {
            _botLogger = botLogger;
            _eventSyncer = eventSyncer;
        }

        public void InjectHost(BotHost<T> instance) { }
        public bool CanHandle(string command)
        {
            return command.StartsWith("req");
        }

        public async Task<bool> Execute(string command)
        {
            await Task.Delay(0);
            foreach (var request in _eventSyncer.Requests)
            {
                _botLogger.LogInfo($"Name:\t{request.Creator.NickName}");
                _botLogger.LogInfo($"Time:\t{request.CreateTime}");
                if (request.Completed)
                {
                    _botLogger.LogSuccess("\tCompleted.");
                }
                else
                {
                    _botLogger.LogWarning("\tPending.");
                }
            }
            return true;
        }
    }
}
