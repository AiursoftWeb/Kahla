using Aiursoft.Pylon.Models.Stargate.ChannelViewModels;

namespace Kahla.Server.Models.ApiViewModels
{
    public class InitPusherViewModel : CreateChannelViewModel
    {
        public string ServerPath { get; set; }
    }
}
