using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.Server.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Services.AppService;

public class ThreadJoinedViewAppService(ThreadJoinedViewRepo repo)
{
    public async Task<(int count, List<KahlaThreadMappedJoinedView> threads)> QueryCommonThreadsAsync(string viewingUserId, string targetUserId, int take)
    {
        var query = repo.QueryCommonThreads(viewingUserId, targetUserId);
        var totalCount = await query.CountAsync();
        var threads = await query.Take(take).ToListAsync();
        return (totalCount, threads);
    }
}