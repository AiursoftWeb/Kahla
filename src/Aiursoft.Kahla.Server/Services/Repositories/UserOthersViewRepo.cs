using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Services.Repositories;

public class UserOthersViewRepo(KahlaDbContext dbContext)
{
    public IQueryable<KahlaUserMappedOthersView> QueryMyContacts(string viewingUserId, int take)
    {
        return dbContext.Users
            .AsNoTracking()
            .Where(t => t.OfKnownContacts.Any(p => p.CreatorId == viewingUserId))
            .MapUsersOthersView(viewingUserId)
            .OrderBy(t => t.User.NickName)
            .Take(take);
    }
        
    public IQueryable<KahlaUserMappedOthersView> QueryMyBlocksPaged(string viewingUserId, int take)
    {
        return dbContext.Users
            .AsNoTracking()
            .Where(t => t.BlockedBy.Any(p => p.CreatorId == viewingUserId))
            .MapUsersOthersView(viewingUserId)
            .OrderBy(t => t.User.NickName)
            .Take(take);
    }

    public IQueryable<KahlaUserMappedOthersView> SearchUsers(string searchInput, string viewingUserId)
    {
        return dbContext.Users
            .AsNoTracking()
            .Where(t => t.AllowSearchByName || t.Id == searchInput)
            .Where(t =>
                t.Email.Contains(searchInput) ||
                t.NickName.Contains(searchInput) ||
                t.Id == searchInput)
            .MapUsersOthersView(viewingUserId)
            .OrderBy(t => t.User.NickName);
    }
    
    public IQueryable<KahlaUserMappedOthersView> QueryUserById(string targetUserId, string viewingUserId)
    {
        return dbContext.Users
            .AsNoTracking()
            .Where(t => t.Id == targetUserId)
            .MapUsersOthersView(viewingUserId);
    }
}