using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Models.Mapped;

namespace Aiursoft.Kahla.Server.Services.Mappers;

public class KahlaThreadMapper
{
    public KahlaThreadMappedSearchedView MapSearchedThread(ChatThread thread)
    {
        return new KahlaThreadMappedSearchedView
        {
            Name = thread.Name,
            ImagePath = thread.IconFilePath,
            OwnerId = thread.OwnerRelation.UserId,
            AllowDirectJoinWithoutInvitation = thread.AllowDirectJoinWithoutInvitation
        };
    }
    
    public async Task<KahlaThreadMappedJoinedView> MapJoinedThread(ChatThread thread)
    {
        await Task.CompletedTask; // TODO: In the future, some properties will be calculated here with await.
        return new KahlaThreadMappedJoinedView
        {
            Name = thread.Name,
            ImagePath = thread.IconFilePath,
            OwnerId = thread.OwnerRelation.UserId,
            AllowDirectJoinWithoutInvitation = thread.AllowDirectJoinWithoutInvitation
        };
    }
}