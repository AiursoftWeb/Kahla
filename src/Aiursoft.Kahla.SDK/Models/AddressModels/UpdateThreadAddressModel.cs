namespace Aiursoft.Kahla.SDK.Models.AddressModels;

public class UpdateThreadAddressModel
{
    public string? Name { get; init; }
    public string? IconFilePath { get; init; }
    public bool? AllowDirectJoinWithoutInvitation { get; init; }
    public bool? AllowMemberSoftInvitation { get; init; }
    public bool? AllowMembersSendMessages { get; init; }
    public bool? AllowMembersEnlistAllMembers { get; init; }
    public bool? AllowSearchByName { get; init; }
}