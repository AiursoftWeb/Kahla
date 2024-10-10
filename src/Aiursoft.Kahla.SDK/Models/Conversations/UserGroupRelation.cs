using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Aiursoft.Kahla.SDK.Models.Conversations;

[Obsolete]
public class UserGroupRelation
{
    [Key]
    public int Id { get; set; }
    public DateTime JoinTime { get; set; } = DateTime.UtcNow;

    public bool Muted { get; set; }

    public required string UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    public required KahlaUser User { get; set; }

    public int GroupId { get; set; }
    [JsonIgnore]
    [ForeignKey(nameof(GroupId))]
    public required GroupConversation Group { get; set; }

    public DateTime ReadTimeStamp { get; set; } = DateTime.UtcNow;
}