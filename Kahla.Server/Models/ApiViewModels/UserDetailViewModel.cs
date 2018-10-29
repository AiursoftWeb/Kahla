using Aiursoft.Pylon.Models.Stargate.ChannelViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aiursoft.Pylon.Models;

namespace Kahla.Server.Models.ApiViewModels
{
    public class UserDetailViewModel : AiurProtocal
    {
        public KahlaUser User { get; set; }
        public bool AreFriends { get; set; }
        public int? ConversationId { get; set; }
    }
}
