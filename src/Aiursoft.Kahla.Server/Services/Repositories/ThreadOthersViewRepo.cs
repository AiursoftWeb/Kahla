using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Services.Repositories;

public class ThreadOthersViewRepo(KahlaDbContext dbContext)
{
    public IOrderedQueryable<KahlaThreadMappedOthersView> SearchThreads(string searchInput, string viewingUserId)
    {
        return dbContext
            .ChatThreads
            .AsNoTracking()
            .Where(t => t.AllowSearchByName || t.Id.ToString() == searchInput)
            .Where(t => 
                t.Name.Contains(searchInput) ||
                t.Id.ToString() == searchInput)
            .MapThreadsOthersView(viewingUserId)
            .OrderByDescending(t => t.CreateTime);
    }
    
    public IQueryable<KahlaThreadMappedOthersView> QueryThreadById(int threadId, string viewingUserId)
    {
        return dbContext
            .ChatThreads
            .AsNoTracking()
            .Where(t => t.Id == threadId)
            .MapThreadsOthersView(viewingUserId);
    }

}