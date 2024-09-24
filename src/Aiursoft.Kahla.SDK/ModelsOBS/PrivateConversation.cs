using System.ComponentModel.DataAnnotations.Schema;
using Aiursoft.Kahla.SDK.Models;

namespace Aiursoft.Kahla.SDK.ModelsOBS
{
    public class PrivateConversation : Conversation
    {
        public required string RequesterId { get; set; }
        [ForeignKey(nameof(RequesterId))]
        public KahlaUser? RequestUser { get; set; }

        public required string TargetId { get; set; }
        [ForeignKey(nameof(TargetId))]
        public KahlaUser? TargetUser { get; set; }

        private KahlaUser? SpecialUser(string myId) => myId == RequesterId ? TargetUser : RequestUser;
        public override string? GetDisplayImagePath(string userId) => SpecialUser(userId)?.IconFilePath;
        public override string? GetDisplayName(string userId) => SpecialUser(userId)?.NickName;
        public override int GetUnReadAmount(string userId) => Messages.Count(p => !p.Read && p.SenderId != userId);

        public override Message? GetLatestMessage()
        {
            return Messages.MaxBy(p => p.SendTime);
        }

        public override void ForEachUser(Action<KahlaUser?, UserGroupRelation?> function)
        {
            function(RequestUser, null);
            if (RequesterId != TargetId)
            {
                function(TargetUser, null);
            }
        }

        public override bool Muted(string userId)
        {
            return false;
        }

        public override Conversation Build(string userId)
        {
            DisplayName = GetDisplayName(userId);
            DisplayImagePath = GetDisplayImagePath(userId);
            return this;
        }

        public override bool HasUser(string userId)
        {
            return RequesterId == userId || TargetId == userId;
        }
    }
}
