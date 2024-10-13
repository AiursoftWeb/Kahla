using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Services.Repositories;

public class ThreadJoinedViewRepo(KahlaDbContext dbContext)
{
    public IQueryable<KahlaThreadMappedJoinedView> QueryThreadsIJoined(string viewingUserId)
    {
        return dbContext.ChatThreads
            .AsNoTracking()
            .Where(t => t.Members.Any(p => p.UserId == viewingUserId))
            .MapThreadsJoinedView(viewingUserId);
    }

    public IQueryable<KahlaThreadMappedJoinedView> QueryCommonThreads(string viewingUserId, string targetUserId)
    {
        return dbContext.ChatThreads
            .AsNoTracking()
            .Where(t => t.Members.Any(p => p.UserId == viewingUserId))
            .Where(t => t.Members.Any(p => p.UserId == targetUserId))
            .MapThreadsJoinedView(viewingUserId);
    }

    public IQueryable<KahlaThreadMappedJoinedView> QueryThreadById(int threadId, string currentUserId)
    {
        return dbContext
            .ChatThreads
            .AsNoTracking()
            .Where(t => t.Id == threadId)
            .MapThreadsJoinedView(currentUserId);
    }
}