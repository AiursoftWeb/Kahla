using System.ComponentModel.DataAnnotations;

namespace Aiursoft.Kahla.SDK.Models.AddressModels;

public class UpdateThreadAddressModel
{
    [StringLength(256, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
    public string? Name { get; init; }
    public string? IconFilePath { get; init; }
    public bool? AllowDirectJoinWithoutInvitation { get; init; }
    public bool? AllowMemberSoftInvitation { get; init; }
    public bool? AllowMembersSendMessages { get; init; }
    public bool? AllowMembersEnlistAllMembers { get; init; }
    public bool? AllowSearchByName { get; init; }
}