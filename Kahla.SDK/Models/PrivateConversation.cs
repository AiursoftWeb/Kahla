using Kahla.SDK.Services;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Kahla.SDK.Models
{
    public class PrivateConversation : Conversation
    {
        public string RequesterId { get; set; }
        [ForeignKey(nameof(RequesterId))]
        public KahlaUser RequestUser { get; set; }

        public string TargetId { get; set; }
        [ForeignKey(nameof(TargetId))]
        public KahlaUser TargetUser { get; set; }
        [NotMapped]
        // Only a property for convenience.
        public string AnotherUserId { get; set; }

        public override KahlaUser SpecialUser(string myId) => myId == RequesterId ? TargetUser : RequestUser;
        public override string GetDisplayImagePath(string userId) => SpecialUser(userId).IconFilePath;
        public override string GetDisplayName(string userId) => SpecialUser(userId).NickName;
        public override int GetUnReadAmount(string userId) => Messages.Count(p => !p.Read && p.SenderId != userId);


        public override Message GetLatestMessage()
        {
            return Messages
                .Where(t => DateTime.UtcNow < t.SendTime + TimeSpan.FromSeconds(t.Conversation.MaxLiveSeconds))
                .OrderByDescending(p => p.SendTime)
                .FirstOrDefault();
        }

        public override void ForEachUser(Action<KahlaUser, UserGroupRelation> function)
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

        public override Conversation Build(string userId, OnlineJudger onlineJudger)
        {
            DisplayName = GetDisplayName(userId);
            DisplayImagePath = GetDisplayImagePath(userId);
            AnotherUserId = SpecialUser(userId).Id;
            RequestUser.Build(onlineJudger);
            TargetUser.Build(onlineJudger);
            return this;
        }

        public override bool HasUser(string userId)
        {
            return RequesterId == userId || TargetId == userId;
        }
    }
}
