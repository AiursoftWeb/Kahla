using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.Server.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Services.AppService;

public class ThreadJoinedViewAppService(
    ILogger<ThreadJoinedViewAppService> logger,
    ThreadJoinedViewRepo repo)
{
    public async Task<(int count, List<KahlaThreadMappedJoinedView> threads)> QueryCommonThreadsAsync(string viewingUserId, string targetUserId, int take)
    {
        var query = repo.QueryCommonThreads(viewingUserId, targetUserId);
        logger.LogInformation("Counting the total common threads between user with id: {ViewingUserId} and user with id: {TargetUserId}.", viewingUserId, targetUserId);
        var totalCount = await query.CountAsync();
        
        logger.LogInformation("User with id: {ViewingUserId} is trying to get common threads with user with id: {TargetUserId}.", viewingUserId, targetUserId);
        var threads = await query
            .OrderByDescending(t => t.CreateTime)
            .Take(take)
            .ToListAsync();
        return (totalCount, threads);
    }
}