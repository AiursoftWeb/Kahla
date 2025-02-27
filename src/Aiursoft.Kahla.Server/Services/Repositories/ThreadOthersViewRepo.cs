using Aiursoft.Kahla.Entities;
using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.Server.Data;
using Aiursoft.Kahla.Server.Services.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Services.Repositories;

public class ThreadOthersViewRepo(KahlaRelationalDbContext relationalDbContext)
{
    public IOrderedQueryable<KahlaThreadMappedOthersView> SearchThreads(
        string? searchInput, 
        string? excluding,
        string viewingUserId)
    {
        return relationalDbContext
            .ChatThreads
            .AsNoTracking()
            .Where(t => t.AllowSearchByName || t.Id.ToString() == searchInput)
            .WhereWhen(excluding, t => 
                !t.Name.Contains(excluding!) &&
                t.Id.ToString() != excluding!)
            .WhereWhen(searchInput, t => 
                t.Name.Contains(searchInput!) ||
                t.Id.ToString() == searchInput)
            .MapThreadsOthersView(viewingUserId)
            .OrderByDescending(t => t.CreateTime);
    }
    
    public IQueryable<KahlaThreadMappedOthersView> QueryThreadById(int threadId, string viewingUserId)
    {
        return relationalDbContext
            .ChatThreads
            .AsNoTracking()
            .Where(t => t.Id == threadId)
            .MapThreadsOthersView(viewingUserId);
    }
}