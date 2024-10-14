using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Services.Repositories;

public class UserOthersViewRepo(KahlaDbContext dbContext, OnlineJudger onlineJudger)
{
    public IOrderedQueryable<KahlaUserMappedOthersView> QueryMyContacts(string viewingUserId)
    {
        return dbContext.Users
            .AsNoTracking()
            .Where(t => t.OfKnownContacts.Any(p => p.CreatorId == viewingUserId))
            .MapUsersOthersView(viewingUserId, onlineJudger)
            .OrderBy(t => t.User.NickName);
    }
    
    public IOrderedQueryable<KahlaUserMappedOthersView> SearchMyContactsAsync(string searchInput, string viewingUserId)
    {
        return dbContext.Users
            .AsNoTracking()
            .Where(t => t.OfKnownContacts.Any(p => p.CreatorId == viewingUserId))
            .Where(t => t.AllowSearchByName || t.Id == searchInput)
            .Where(t =>
                t.Email.Contains(searchInput) ||
                t.NickName.Contains(searchInput) ||
                t.Id == searchInput)
            .MapUsersOthersView(viewingUserId, onlineJudger)
            .OrderBy(t => t.User.NickName);
    }
        
    public IOrderedQueryable<KahlaUserMappedOthersView> QueryMyBlocksPaged(string viewingUserId)
    {
        return dbContext.Users
            .AsNoTracking()
            .Where(t => t.BlockedBy.Any(p => p.CreatorId == viewingUserId))
            .MapUsersOthersView(viewingUserId, onlineJudger)
            .OrderBy(t => t.User.NickName);
    }
    
    public IOrderedQueryable<KahlaUserMappedOthersView> SearchMyBlocksAsync(string searchInput, string viewingUserId)
    {
        return dbContext.Users
            .AsNoTracking()
            .Where(t => t.BlockedBy.Any(p => p.CreatorId == viewingUserId))
            .Where(t => t.AllowSearchByName || t.Id == searchInput)
            .Where(t =>
                t.Email.Contains(searchInput) ||
                t.NickName.Contains(searchInput) ||
                t.Id == searchInput)
            .MapUsersOthersView(viewingUserId, onlineJudger)
            .OrderBy(t => t.User.NickName);
    }

    public IOrderedQueryable<KahlaUserMappedOthersView> SearchUsers(string searchInput, string viewingUserId)
    {
        return dbContext.Users
            .AsNoTracking()
            .Where(t => t.AllowSearchByName || t.Id == searchInput)
            .Where(t =>
                t.Email.Contains(searchInput) ||
                t.NickName.Contains(searchInput) ||
                t.Id == searchInput)
            .MapUsersOthersView(viewingUserId, onlineJudger)
            .OrderBy(t => t.User.NickName);
    }
    
    public IQueryable<KahlaUserMappedOthersView> QueryUserById(string targetUserId, string viewingUserId)
    {
        return dbContext.Users
            .AsNoTracking()
            .Where(t => t.Id == targetUserId)
            .MapUsersOthersView(viewingUserId, onlineJudger);
    }
}