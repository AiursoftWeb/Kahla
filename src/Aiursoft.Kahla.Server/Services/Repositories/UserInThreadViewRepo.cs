using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.Server.Data;
using Aiursoft.Kahla.Server.Services.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Services.Repositories;

public class UserInThreadViewRepo(KahlaRelationalDbContext relationalDbContext, OnlineDetector onlineDetector)
{
    public IOrderedQueryable<KahlaUserMappedInThreadView> QueryMembersInThread(int threadId, string viewingUserId)
    {
        return relationalDbContext.UserThreadRelations
            .AsNoTracking()
            .Where(t => t.ThreadId == threadId)
            .MapUsersInThreadView(viewingUserId, onlineDetector)
            .OrderBy(t => t.JoinTime);
    }
}