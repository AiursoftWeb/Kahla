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
}