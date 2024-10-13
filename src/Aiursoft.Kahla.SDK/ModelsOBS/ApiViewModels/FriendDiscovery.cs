using Aiursoft.Kahla.SDK.Models.Entities;

namespace Aiursoft.Kahla.SDK.ModelsOBS.ApiViewModels
{
    public class FriendDiscovery
    {
        public int CommonFriends { get; set; }
        public int CommonGroups { get; set; }
        public bool SentRequest { get; set; }
        public required KahlaUser TargetUser { get; set; }
    }
}
