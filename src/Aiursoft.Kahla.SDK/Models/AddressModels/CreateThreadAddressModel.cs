namespace Aiursoft.Kahla.SDK.Models.AddressModels;

public class CreateThreadAddressModel
{
    public required string Name { get; init; }
    
    public required bool AllowSearchByName { get; init; }
    
    public required bool AllowDirectJoinWithoutInvitation { get; init; }
    
    public required bool AllowMemberSoftInvitation { get; init; }
    
    public required bool AllowMembersSendMessages { get; init; } = true;
    
    public required bool AllowMembersEnlistAllMembers { get; init; } = true;
}