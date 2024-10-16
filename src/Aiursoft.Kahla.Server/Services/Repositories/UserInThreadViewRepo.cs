using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Services.Repositories;

public class UserInThreadViewRepo(KahlaDbContext dbContext, OnlineJudger onlineJudger)
{
    public IOrderedQueryable<KahlaUserMappedInThreadView> QueryMembersInThread(int threadId, string viewingUserId)
    {
        return dbContext.UserThreadRelations
            .AsNoTracking()
            .Where(t => t.ThreadId == threadId)
            .MapUsersInThreadView(viewingUserId, onlineJudger)
            .OrderBy(t => t.JoinTime);
    }
}