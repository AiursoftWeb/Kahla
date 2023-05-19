using System.Collections.Generic;

namespace Kahla.SDK.Models
{
    public class EmailReason
    {
        // ReSharper disable once CollectionNeverQueried.Global
        public List<int> UnreadInConversationIds { get; set; } = new List<int>();

        // ReSharper disable once CollectionNeverQueried.Global
        public List<int> UnreadFriendRequestIds { get; set; } = new List<int>();
    }
}
