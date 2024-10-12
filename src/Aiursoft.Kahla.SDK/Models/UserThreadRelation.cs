using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Aiursoft.Kahla.SDK.Models;

/// <summary>
/// For each user in a thread, there will be a UserthreadRelation. And he will be treated as a member of this thread.
/// </summary>
public class UserThreadRelation
{
    [Key]
    public int Id { get; set; }
    public DateTime JoinTime { get; set; } = DateTime.UtcNow;

    public UserThreadRole UserThreadRole { get; set; } = UserThreadRole.Member;
    
    public bool Muted { get; set; }
    
    /// <summary>
    /// If a user is banned from a group, this will be true, and he won't be able to send messages to this group. However, he is still able to read messages.
    /// </summary>
    public bool Baned { get; set; }

    public required string UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    [NotNull]
    public KahlaUser? User { get; set; }

    public required int ThreadId { get; set; }
    [JsonIgnore]
    [ForeignKey(nameof(ThreadId))]
    [NotNull]
    public ChatThread? Thread { get; set; }

    public DateTime ReadTimeStamp { get; set; } = DateTime.UtcNow;
}