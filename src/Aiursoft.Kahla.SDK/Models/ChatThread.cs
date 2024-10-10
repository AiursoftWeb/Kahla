using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// ReSharper disable RedundantDefaultMemberInitializer

namespace Aiursoft.Kahla.SDK.Models;

public class ChatThread
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// The name of this thread. This name will be shown in the chat list.
    ///
    /// Admins can change this value.
    /// </summary>
    [StringLength(256)] public required string Name { get; set; } = "{THE OTHER USER}";
    
    /// <summary>
    /// The icon of this thread. This icon will be shown in the chat list.
    ///
    /// Admins can change this value.
    /// </summary>
    [StringLength(512)]
    public required string IconFilePath { get; set; } = "{THE OTHER USER ICON}";
    
    [InverseProperty(nameof(UserThreadRelation.Thread))]
    public IEnumerable<UserThreadRelation> Members { get; set; } = new List<UserThreadRelation>();
    
    /// <summary>
    /// Indicating the owner of this thread.
    ///
    /// The owner must be a member of this thread.
    ///
    /// Only the owner can transfer the ownership of this thread to another member. (Admins can NOT do this)
    ///
    /// Only the owner can change the roles of the members. (Admins can NOT do this)
    /// </summary>
    public required int OwnerRelationId { get; set; }
    [ForeignKey(nameof(OwnerRelationId))]
    public required UserThreadRelation OwnerRelation { get; set; }
    
    /// <summary>
    /// Indicating if allowing a user to join this thread without a SoftInvitation. By default, this is false.
    ///
    /// Admins can change this value.
    /// </summary>
    public bool AllowDirectJoinWithoutInvitation { get; set; } = false;
    
    /// <summary>
    /// Indicating if allowing a user to search this thread by name. By default, this is false.
    ///
    /// Admins can change this value.
    /// </summary>
    public bool AllowSearchByName { get; set; } = false;
    
    /// <summary>
    /// If this is true, then a member can invite a user to join this thread via SoftInvitation. By default, this is false.
    ///
    /// No matter if this is true or false, the admins can always invite a user to join this thread via SoftInvitation.
    ///
    /// Admins can change this value.
    /// </summary>
    public bool AllowMemberSoftInvitation { get; set; } = false;
    
    /// <summary>
    /// If this is true, then all members can send messages to this thread. By default, this is true.
    ///
    /// No matter if this is true or false, the admins can always send messages to this thread.
    ///
    /// Admins can change this value.
    /// </summary>
    public bool AllowMembersSendMessages { get; set; } = true;
    
    /// <summary>
    /// If this is true, then all members can see all the other members in this thread. By default, this is true.
    ///
    /// If this is false, then non-admin members can only see the owner of this thread.
    ///
    /// No matter if this is true or false, the admins can always see all the other members in this thread.
    ///
    /// Admins can change this value.
    /// </summary>
    public bool AllowMembersEnlistAllMembers { get; set; } = true;
}