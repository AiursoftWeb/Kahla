using Aiursoft.Kahla.SDK.Models.Entities;
using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Services.Repositories;

public class ThreadJoinedViewRepo(
    KahlaDbContext dbContext, 
    OnlineJudger judger,
    QuickMessageAccess quickMessageAccess)
{
    public IOrderedQueryable<KahlaThreadMappedJoinedView> SearchThreadsIJoined(
        string? searchInput,
        string? excluding,
        string viewingUserId)
    {
        return dbContext.ChatThreads
            .AsNoTracking()
            .Where(t => t.Members.Any(p => p.UserId == viewingUserId))
            .WhereWhen(excluding, t => !t.Name.Contains(excluding!))
            .WhereWhen(searchInput, t => t.Name.Contains(searchInput!))
            .MapThreadsJoinedView(viewingUserId, judger, quickMessageAccess)
            .OrderByDescending(t => t.LastMessageTime);
    }

    public IOrderedQueryable<KahlaThreadMappedJoinedView> QueryCommonThreads(string viewingUserId, string targetUserId)
    {
        return dbContext.ChatThreads
            .AsNoTracking()
            .Where(t => t.Members.Any(p => p.UserId == viewingUserId))
            .Where(t => t.Members.Any(p => p.UserId == targetUserId))
            .MapThreadsJoinedView(viewingUserId, judger, quickMessageAccess)
            .OrderByDescending(t => t.LastMessageTime);
    }
    
    public IQueryable<ChatThread> QueryOnlyUsThreads(string viewingUserId, string targetUserId)
    {
        return dbContext.ChatThreads
            .AsNoTracking()
            .Where(t => t.Members.All(p => p.UserId == viewingUserId || p.UserId == targetUserId));
    }

    public IQueryable<KahlaThreadMappedJoinedView> QueryThreadById(int threadId, string currentUserId)
    {
        return dbContext.ChatThreads
            .AsNoTracking()
            .Where(t => t.Id == threadId)
            .MapThreadsJoinedView(currentUserId, judger, quickMessageAccess);
    }
}