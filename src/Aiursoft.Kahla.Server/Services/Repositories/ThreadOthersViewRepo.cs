using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Services.Repositories;

public class ThreadOthersViewRepo(KahlaDbContext dbContext)
{
    private IQueryable<KahlaThreadMappedOthersView> MapThreadsOthersView(IQueryable<ChatThread> filteredThreads, string userId)
    {
        return filteredThreads
            .AsNoTracking()
            .Select(t => new KahlaThreadMappedOthersView
            {
                Id = t.Id,
                Name = t.Name,
                ImagePath = t.IconFilePath,
                OwnerId = t.OwnerRelation.UserId,
                AllowDirectJoinWithoutInvitation = t.AllowDirectJoinWithoutInvitation,
                CreateTime = t.CreateTime,
                ImInIt = t.Members.Any(u => u.UserId == userId)
            });
    }
    
    public IQueryable<KahlaThreadMappedOthersView> SearchThreads(string searchInput, string userId)
    {
        var threadsQuery = dbContext
            .ChatThreads
            .AsNoTracking()
            .Where(t => t.AllowSearchByName || t.Id.ToString() == searchInput)
            .Where(t => 
                t.Name.Contains(searchInput) ||
                t.Id.ToString() == searchInput);

        return MapThreadsOthersView(threadsQuery, userId)
            .OrderByDescending(t => t.CreateTime);
    }
}