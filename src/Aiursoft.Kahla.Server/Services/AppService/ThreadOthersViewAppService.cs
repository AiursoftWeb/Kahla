using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.Server.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Services.AppService;

public class ThreadOthersViewAppService(ThreadOthersViewRepo repo)
{
    public async Task<(int count, List<KahlaThreadMappedOthersView> threads)> SearchThreadsPagedAsync(
        string? searchInput, 
        string? excluding, 
        string viewingUserId, 
        int skip, 
        int take)
    {
        var query = repo.SearchThreads(searchInput, excluding, viewingUserId);
        var totalCount = await query.CountAsync();
        var threads = await query.Skip(skip).Take(take).ToListAsync();
        return (totalCount, threads);
    }
    
    public async Task<KahlaThreadMappedOthersView?> GetThreadAsync(int threadId, string viewingUserId)
    {
        var thread = await repo
            .QueryThreadById(threadId, viewingUserId)
            .FirstOrDefaultAsync();
        return thread;
    }
}