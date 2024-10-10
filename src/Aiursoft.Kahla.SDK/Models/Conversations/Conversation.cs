using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Aiursoft.Kahla.SDK.ModelsOBS;

namespace Aiursoft.Kahla.SDK.Models.Conversations
{
    [Obsolete]
    public abstract class Conversation
    {
        // Properties
        [Key]
        public int Id { get; set; }
        public string? Discriminator { get; set; }
        
        [Obsolete]
        [NotMapped]
        public IEnumerable<Message> Messages { get; set; } = new List<Message>();

        public DateTime ConversationCreateTime { get; set; } = DateTime.UtcNow;

        // Abstract methods
        public abstract string? GetDisplayName(string userId);
        public abstract string? GetDisplayImagePath(string userId);
        public abstract int GetUnReadAmount(string userId);
        public abstract bool Muted(string userId);
        public abstract Message? GetLatestMessage();
        public abstract void ForEachUser(Action<KahlaUser?, UserGroupRelation?> function);
        public abstract bool HasUser(string userId);
    }
}
