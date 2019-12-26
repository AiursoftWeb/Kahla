using System;
using System.Collections.Generic;
using System.Text;

namespace Kahla.SDK.Models
{
    public class EmailReason
    {
        public List<int> UnreadInConversationIds { get; set; } = new List<int>();
        public List<int> UnreadFriendRequestIds { get; set; } = new List<int>();
    }
}
