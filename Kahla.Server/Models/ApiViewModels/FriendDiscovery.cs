using Aiursoft.Pylon.Models.Stargate.ChannelViewModels;

namespace Kahla.Server.Models.ApiViewModels
{
    public class FriendDiscovery : CreateChannelViewModel
    {
        public int CommonFriends { get; set; }
        public int CommonGroups { get; set; }
        public bool SentRequest { get; set; }
        public KahlaUser TargetUser { get; set; }
    }
}
