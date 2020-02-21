using Kahla.SDK.Abstract;

namespace Kahla.SDK.CommandHandlers
{
    [CommandHandler("conv")]
    public class ConvCommandHandler : CommandHandlerBase
    {
        public ConvCommandHandler(BotCommander botCommander) : base(botCommander)
        {
        }
    }
}
