using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Services.Repositories;

public class UserInThreadViewRepo(KahlaDbContext dbContext)
{
    public IOrderedQueryable<KahlaUserMappedInThreadView> QueryMembersInThread(int threadId, string viewingUserId)
    {
        return dbContext.Users
            .AsNoTracking()
            .Where(t => t.ThreadsRelations.Any(p => p.ThreadId == threadId))
            .MapUsersInThreadView(viewingUserId, threadId)
            .OrderBy(t => t.User.NickName);
    }
}