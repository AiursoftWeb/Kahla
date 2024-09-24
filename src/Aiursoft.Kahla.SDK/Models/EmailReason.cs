namespace Kahla.SDK.Models
{
    public class EmailReason
    {
        // ReSharper disable once CollectionNeverQueried.Global
        public List<int> UnreadInConversationIds { get; set; } = new();

        // ReSharper disable once CollectionNeverQueried.Global
        public List<int> UnreadFriendRequestIds { get; set; } = new();
    }
}
