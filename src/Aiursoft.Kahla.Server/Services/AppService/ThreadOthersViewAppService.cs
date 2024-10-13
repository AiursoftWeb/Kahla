using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.Server.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Services.AppService;

public class ThreadOthersViewAppService(ThreadOthersViewRepo repo)
{
    public async Task<(int count, List<KahlaThreadMappedOthersView> users)> SearchThreadsPagedAsync(string searchInput, string viewingUserId, int take)
    {
        var query = repo.SearchThreads(searchInput, viewingUserId);
        var totalCount = await query.CountAsync();
        var threads = await query.Take(take).ToListAsync();
        return (totalCount, threads);
    }
}