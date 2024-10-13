using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Aiursoft.Kahla.SDK.Models.Entities;

/// <summary>
/// For each user in a thread, there will be a UserthreadRelation. And he will be treated as a member of this thread.
/// </summary>
public class UserThreadRelation
{
    [Key]
    public int Id { get; init; }
    public DateTime JoinTime { get; init; } = DateTime.UtcNow;

    public UserThreadRole UserThreadRole { get; init; } = UserThreadRole.Member;
    
    public bool Muted { get; init; }
    
    /// <summary>
    /// If a user is banned from a group, this will be true, and he won't be able to send messages to this group. However, he is still able to read messages.
    /// </summary>
    public bool Baned { get; init; }

    public required string UserId { get; init; }
    [ForeignKey(nameof(UserId))]
    [NotNull]
    public KahlaUser? User { get; init; }

    public required int ThreadId { get; init; }
    [JsonIgnore]
    [ForeignKey(nameof(ThreadId))]
    [NotNull]
    public ChatThread? Thread { get; init; }

    public DateTime ReadTimeStamp { get; init; } = DateTime.UtcNow;
}