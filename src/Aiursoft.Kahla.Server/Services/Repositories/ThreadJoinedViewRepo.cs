using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Services.Repositories;

public class ThreadJoinedViewRepo(KahlaDbContext dbContext)
{
    public IOrderedQueryable<KahlaThreadMappedJoinedView> QueryThreadsIJoined(string viewingUserId)
    {
        return dbContext.ChatThreads
            .AsNoTracking()
            .Where(t => t.Members.Any(p => p.UserId == viewingUserId))
            .MapThreadsJoinedView(viewingUserId)
            .OrderByDescending(t => t.LastMessageTime);
    }

    public IOrderedQueryable<KahlaThreadMappedJoinedView> QueryCommonThreads(string viewingUserId, string targetUserId)
    {
        return dbContext.ChatThreads
            .AsNoTracking()
            .Where(t => t.Members.Any(p => p.UserId == viewingUserId))
            .Where(t => t.Members.Any(p => p.UserId == targetUserId))
            .MapThreadsJoinedView(viewingUserId)
            .OrderByDescending(t => t.LastMessageTime);
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