using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Kahla.Server.Models
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

        public KahlaUser AnotherUser(string myId) => myId == RequesterId ? TargetUser : RequestUser;
        public override int GetDisplayImage(string userId) => AnotherUser(userId).HeadImgFileKey;
        public override string GetDisplayName(string userId) => AnotherUser(userId).NickName;
        public override int GetUnReadAmount(string userId) => Messages.Count(p => !p.Read && p.SenderId != userId);
        public override Message GetLatestMessage()
        {
            try
            {
                return Messages.OrderByDescending(p => p.SendTime).First();
            }
            catch (InvalidOperationException)
            {
                return new Message
                {
                    Content = null,//"You are friends. Start chatting now!",
                    SendTime = ConversationCreateTime
                };
            }
        }

    }
}
