using System.ComponentModel.DataAnnotations.Schema;
using Aiursoft.Kahla.SDK.ModelsOBS;

namespace Aiursoft.Kahla.SDK.Models.Conversations
{
    public class PrivateConversation : Conversation
    {
        // Properties
        public required string RequesterId { get; set; }
        [ForeignKey(nameof(RequesterId))]
        public KahlaUser? RequestUser { get; set; }

        public required string TargetId { get; set; }
        [ForeignKey(nameof(TargetId))]
        public KahlaUser? TargetUser { get; set; }

        // Overrides
        private KahlaUser? GetTheOtherUser(string myId) => myId == RequesterId ? TargetUser : RequestUser;
        public override string? GetDisplayImagePath(string userId) => GetTheOtherUser(userId)?.IconFilePath;
        public override string? GetDisplayName(string userId) => GetTheOtherUser(userId)?.NickName;
        public override int GetUnReadAmount(string userId) => Messages.Count(p => !p.Read && p.SenderId != userId);
        public override bool Muted(string userId) => false;
        public override Message? GetLatestMessage() => Messages.MaxBy(p => p.SendTime);
        public override bool HasUser(string userId) => RequesterId == userId || TargetId == userId; 
        public override void ForEachUser(Action<KahlaUser?, UserGroupRelation?> function)
        {
            function(RequestUser, null);
            if (RequesterId != TargetId)
            {
                function(TargetUser, null);
            }
        }
    }
}
