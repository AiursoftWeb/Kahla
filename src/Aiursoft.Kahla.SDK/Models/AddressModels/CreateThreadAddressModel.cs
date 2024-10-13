using System.ComponentModel.DataAnnotations;

namespace Aiursoft.Kahla.SDK.Models.AddressModels;

public class CreateThreadAddressModel
{
    [Required]
    [StringLength(256, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
    public required string Name { get; init; }
    [Required]
    public required bool AllowSearchByName { get; init; }
    [Required]
    public required bool AllowDirectJoinWithoutInvitation { get; init; }
    [Required]
    public required bool AllowMemberSoftInvitation { get; init; }
    [Required]
    public required bool AllowMembersSendMessages { get; init; } = true;
    [Required]
    public required bool AllowMembersEnlistAllMembers { get; init; } = true;
}