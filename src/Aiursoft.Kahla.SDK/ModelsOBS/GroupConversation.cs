using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Aiursoft.Kahla.SDK.ModelsOBS
{
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
            var relation = Users.SingleOrDefault(t => t.UserId == userId);
            if (relation == null)
            {
                return 0;
            }
            return Messages.Count(t => t.SendTime > relation.ReadTimeStamp);
        }

        public override Message? GetLatestMessage()
        {
            return Messages.MaxBy(p => p.SendTime);
        }

        public override void ForEachUser(Action<KahlaUser, UserGroupRelation> function)
        {
            foreach (var relation in Users)
            {
                function(relation.User, relation);
            }
        }

        public override bool Muted(string userId)
        {
            return Users.SingleOrDefault(t => t.UserId == userId)?.Muted ?? throw new ArgumentNullException();
        }

        public override Conversation Build(string userId)
        {
            DisplayName = GetDisplayName(userId);
            DisplayImagePath = GetDisplayImagePath(userId);
            Users = Users.OrderByDescending(t => t.UserId == OwnerId).ThenBy(t => t.JoinTime);
            return this;
        }

        public override bool HasUser(string userId)
        {
            return Users.Any(t => t.UserId == userId);
        }
    }
}
