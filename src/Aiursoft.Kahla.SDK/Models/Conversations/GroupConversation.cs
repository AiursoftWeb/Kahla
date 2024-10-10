using System.ComponentModel.DataAnnotations.Schema;
using Aiursoft.Kahla.SDK.ModelsOBS;
using Newtonsoft.Json;

namespace Aiursoft.Kahla.SDK.Models.Conversations
{
    [Obsolete]
    public class GroupConversation : Conversation
    {
        [InverseProperty(nameof(UserGroupRelation.Group))]
        public IEnumerable<UserGroupRelation> Users { get; set; } = new List<UserGroupRelation>();
        
        public required string GroupImagePath { get; set; }
        public required string GroupName { get; set; }
        
        [JsonIgnore]
        public required string JoinPassword { get; set; }
        
        public bool ListInSearchResult { get; set; } = true;

        [JsonProperty]
        [NotMapped]
        public bool HasPassword => !string.IsNullOrEmpty(JoinPassword);

        public required string OwnerId { get; set; }
        [JsonIgnore]
        [ForeignKey(nameof(OwnerId))]
        public KahlaUser? Owner { get; set; }

        public override string GetDisplayName(string userId) => GroupName;
        public override string GetDisplayImagePath(string userId) => GroupImagePath;
        public override int GetUnReadAmount(string userId)
        {
            var relation = Users.FirstOrDefault(t => t.UserId == userId);
            if (relation == null)
            {
                throw new InvalidOperationException("Users are not loaded from database but tried to get unread amount!");
            }
            return Messages.Count(t => t.SendTime > relation.ReadTimeStamp);
        }
        public override bool Muted(string userId)
        {
            return Users.SingleOrDefault(t => t.UserId == userId)?.Muted ?? throw new ArgumentNullException();
        }
        public override Message? GetLatestMessage() => Messages.MaxBy(p => p.SendTime);
        public override bool HasUser(string userId) => Users.Any(t => t.UserId == userId);
        public override void ForEachUser(Action<KahlaUser, UserGroupRelation> function)
        {
            foreach (var relation in Users)
            {
                function(relation.User, relation);
            }
        }
    }
}
