using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.Server.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Services.AppService;

public class ThreadJoinedViewAppService(
    ThreadJoinedViewRepo repo)
{
    public async Task<(int totalCount, List<KahlaThreadMappedJoinedView> threads)> QueryCommonThreadsAsync(string viewingUserId, string targetUserId, int skip, int take)
    {
        var query = repo.QueryCommonThreads(viewingUserId, targetUserId);
        var totalCount = await query.CountAsync();
        var threads = await query
            .Skip(skip)
            .Take(take)
            .ToListAsync();
        return (totalCount, threads);
    }
    
    public async Task<int?> QueryDefaultAsync(string viewingUserId, string targetUserId)
    {
        var firstDefaultThread = await repo
            .QueryOnlyUsThreads(viewingUserId, targetUserId)
            .FirstOrDefaultAsync(t => t.Members.Count() <= 2);
      
        return firstDefaultThread?.Id;
    }
    
    public async Task<(int totalCount, List<KahlaThreadMappedJoinedView> threads)> SearchThreadsIJoinedAsync(string? searchInput, string? excluding, string viewingUserId, int skip, int take)
    {
        var query = repo.SearchThreadsIJoined(searchInput, excluding, viewingUserId);
        var totalCount = await query.CountAsync();
        var threads = await query
            .Skip(skip)
            .Take(take)
            .ToListAsync();
        return (totalCount, threads);
    }

    public async Task<KahlaThreadMappedJoinedView> GetJoinedThreadAsync(int threadId, string viewingUserId)
    {
        var thread = await repo
            .QueryThreadById(threadId, viewingUserId)
            .FirstOrDefaultAsync();
        return thread!; // This can NOT be null. Because if the user didn't join the thread, it's already thrown an exception.
    }
}