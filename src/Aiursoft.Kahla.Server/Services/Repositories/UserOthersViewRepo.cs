using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Services.Repositories;

public class UserOthersViewRepo(KahlaDbContext dbContext)
{
    private IQueryable<KahlaUserMappedOthersView> MapUsersOthersView(IQueryable<KahlaUser> filteredUsers, string viewingUserId)
    {
        return filteredUsers
            .Select(t => new KahlaUserMappedOthersView
            {
                User = t,
                Online = null, // This needed to be calculated in real-time.
                IsKnownContact = t.OfKnownContacts.Any(p => p.CreatorId == viewingUserId),
                IsBlockedByYou = t.BlockedBy.Any(p => p.CreatorId == viewingUserId)
            });
    }
    
    public IQueryable<KahlaUserMappedOthersView> QueryMyContacts(string viewingUserId, int take)
    {
        var query = dbContext.Users
            .AsNoTracking()
            .Where(t => t.OfKnownContacts.Any(p => p.CreatorId == viewingUserId));
            
        return MapUsersOthersView(query, viewingUserId)
            .OrderBy(t => t.User.NickName)
            .Take(take);
    }
        
    public IQueryable<KahlaUserMappedOthersView> QueryMyBlocksPaged(string viewingUserId, int take)
    {
        var query = dbContext.Users
            .AsNoTracking()
            .Where(t => t.BlockedBy.Any(p => p.CreatorId == viewingUserId));
            
        return MapUsersOthersView(query, viewingUserId)
            .OrderBy(t => t.User.NickName)
            .Take(take);
    }

    public IQueryable<KahlaUserMappedOthersView> SearchUsers(string searchInput, string viewingUserId)
    {
        var query = dbContext.Users
            .AsNoTracking()
            .Where(t => t.AllowSearchByName || t.Id == searchInput)
            .Where(t =>
                t.Email.Contains(searchInput) ||
                t.NickName.Contains(searchInput) ||
                t.Id == searchInput);

        return MapUsersOthersView(query, viewingUserId)
            .OrderBy(t => t.User.NickName);
    }
    
    public IQueryable<KahlaUserMappedOthersView> QueryUserById(string targetUserId, string viewingUserId)
    {
        var query = dbContext.Users
            .AsNoTracking()
            .Where(t => t.Id == targetUserId);
        
        return MapUsersOthersView(query, viewingUserId);
    }
}