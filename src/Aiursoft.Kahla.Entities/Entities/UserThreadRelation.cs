using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Aiursoft.Kahla.SDK.Models;
using Newtonsoft.Json;

namespace Aiursoft.Kahla.Entities.Entities;

/// <summary>
/// For each user in a thread, there will be a UserThreadRelation. And he will be treated as a member of this thread.
/// </summary>
public class UserThreadRelation
{
    [Key]
    public int Id { get; init; }
    public DateTime JoinTime { get; init; } = DateTime.UtcNow;

    public UserThreadRole UserThreadRole { get; set; } = UserThreadRole.Member;

    public bool Muted { get; set; }

    /// <summary>
    /// If a user is banned from a group, this will be true, and he won't be able to send messages to this group. However, he is still able to read messages.
    /// </summary>
    public bool Baned { get; init; }

    [StringLength(64)]
    public required string UserId { get; init; }
    [ForeignKey(nameof(UserId))]
    [NotNull]
    public KahlaUser? User { get; init; }

    public required int ThreadId { get; init; }
    [JsonIgnore]
    [ForeignKey(nameof(ThreadId))]
    [NotNull]
    public ChatThread? Thread { get; init; }

    public int ReadMessageIndex { get; set; }
}
