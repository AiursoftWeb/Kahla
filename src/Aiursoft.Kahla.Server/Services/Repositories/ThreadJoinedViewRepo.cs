using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.Server.Data;
using Aiursoft.Kahla.Server.Models.Entities;
using Aiursoft.Kahla.Server.Services.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Services.Repositories;

public class ThreadJoinedViewRepo(
    KahlaRelationalDbContext relationalDbContext,
    OnlineDetector detector,
    QuickMessageAccess quickMessageAccess)
{
    public IOrderedQueryable<KahlaThreadMappedJoinedView> SearchThreadsIJoined(
        string? searchInput,
        string? excluding,
        string viewingUserId)
    {
        return relationalDbContext.ChatThreads
            .AsNoTracking()
            .Where(t => t.Members.Any(p => p.UserId == viewingUserId))
            .WhereWhen(excluding, t => !t.Name.Contains(excluding!))
            .WhereWhen(searchInput, t =>
                t.Name.Contains(searchInput!) ||
                t.Id.ToString() == searchInput ||
                t.Members.Any(p => p.User.NickName.Contains(searchInput!)) ||
                t.Members.Any(p => p.User.Email.Contains(searchInput!)) ||
                t.Members.Any(p => p.User.Id == searchInput))
            .MapThreadsJoinedView(viewingUserId, detector, quickMessageAccess)
            .OrderByDescending(t => t.CreateTime);
    }

    public IOrderedQueryable<KahlaThreadMappedJoinedView> GetThreadsIJoined(int[] threadIds, string viewingUserId)
    {
        return relationalDbContext.ChatThreads
            .AsNoTracking()
            .Where(t => threadIds.Contains(t.Id))
            .MapThreadsJoinedView(viewingUserId, detector, quickMessageAccess)
            .OrderByDescending(t => t.CreateTime);
    }

    public IOrderedQueryable<KahlaThreadMappedJoinedView> QueryCommonThreads(string viewingUserId, string targetUserId)
    {
        return relationalDbContext.ChatThreads
            .AsNoTracking()
            .Where(t => t.Members.Any(p => p.UserId == viewingUserId))
            .Where(t => t.Members.Any(p => p.UserId == targetUserId))
            .MapThreadsJoinedView(viewingUserId, detector, quickMessageAccess)
            .OrderByDescending(t => t.CreateTime);
    }

    public IQueryable<ChatThread> QueryOnlyUsThreads(string viewingUserId, string targetUserId)
    {
        return relationalDbContext.ChatThreads
            .AsNoTracking()
            .Where(t => t.Members.All(p => p.UserId == viewingUserId || p.UserId == targetUserId));
    }

    public IQueryable<KahlaThreadMappedJoinedView> QueryThreadById(int threadId, string currentUserId)
    {
        return relationalDbContext.ChatThreads
            .AsNoTracking()
            .Where(t => t.Id == threadId)
            .MapThreadsJoinedView(currentUserId, detector, quickMessageAccess);
    }
}