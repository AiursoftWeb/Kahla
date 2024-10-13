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
            .OrderByDescending(t => t.LastMessageTime)
            .Take(take)
            .ToListAsync();
        return (totalCount, threads);
    }
    
    public async Task<(int totalCount, List<KahlaThreadMappedJoinedView> threads)> QueryThreadsIJoinedAsync(string viewingUserId, int skip, int take)
    {
        var query = repo.QueryThreadsIJoined(viewingUserId);
        var totalCount = await query.CountAsync();
        var threads = await query
            .OrderByDescending(t => t.LastMessageTime)
            .Take(take)
            .ToListAsync();
        return (totalCount, threads);
    }
}