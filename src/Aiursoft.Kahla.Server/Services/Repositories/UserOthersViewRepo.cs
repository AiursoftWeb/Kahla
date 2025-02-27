using Aiursoft.Canon;
using Aiursoft.Kahla.Entities;
using Aiursoft.Kahla.Entities.Entities;
using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.Server.Services.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Services.Repositories;

public class UserOthersViewRepo(
    KahlaRelationalDbContext relationalDbContext, 
    CacheService cache,
    OnlineDetector onlineDetector)
{
    public IOrderedQueryable<KahlaUserMappedOthersView> SearchMyContactsAsync(
        string? searchInput,
        string? excluding,
        string viewingUserId)
    {
        return relationalDbContext.Users
            .AsNoTracking()
            .Where(t => t.OfKnownContacts.Any(p => p.CreatorId == viewingUserId)) // Allow searching for users even he disabled search by name.
            .WhereWhen(excluding, t => 
                !t.Email.Contains(excluding!) &&
                !t.NickName.Contains(excluding!) &&
                t.Id != excluding!)
            .WhereWhen(searchInput, t =>
                t.Email.Contains(searchInput!) ||
                t.NickName.Contains(searchInput!) ||
                t.Id == searchInput)
            .MapUsersOthersView(viewingUserId, onlineDetector)
            .OrderBy(t => t.User.NickName);
    }
        
    public IOrderedQueryable<KahlaUserMappedOthersView> SearchMyBlocksAsync(
        string? searchInput,
        string? excluding,
        string viewingUserId)
    {
        return relationalDbContext.Users
            .AsNoTracking()
            .Where(t => t.BlockedBy.Any(p => p.CreatorId == viewingUserId)) // Allow searching for users even he disabled search by name.
            .WhereWhen(excluding, t => 
                !t.Email.Contains(excluding!) &&
                !t.NickName.Contains(excluding!) &&
                t.Id != excluding!)
            .WhereWhen(searchInput, t =>
                t.Email.Contains(searchInput!) ||
                t.NickName.Contains(searchInput!) ||
                t.Id == searchInput)
            .MapUsersOthersView(viewingUserId, onlineDetector)
            .OrderBy(t => t.User.NickName);
    }

    public IOrderedQueryable<KahlaUserMappedOthersView> SearchUsers(
        string? searchInput,
        string? excluding,
        string viewingUserId)
    {
        return relationalDbContext.Users
            .AsNoTracking()
            .Where(t => t.AllowSearchByName || t.Id == searchInput) // Only allow searching for users who allow search by name.
            .WhereWhen(excluding, t => 
                !t.Email.Contains(excluding!) &&
                !t.NickName.Contains(excluding!) &&
                t.Id != excluding!)
            .WhereWhen(searchInput, t =>
                t.Email.Contains(searchInput!) ||
                t.NickName.Contains(searchInput!) ||
                t.Id == searchInput)
            .MapUsersOthersView(viewingUserId, onlineDetector)
            .OrderBy(t => t.User.NickName);
    }
    
    public IQueryable<KahlaUserMappedOthersView> QueryUserById(string targetUserId, string viewingUserId)
    {
        return relationalDbContext.Users
            .AsNoTracking()
            .Where(t => t.Id == targetUserId)
            .MapUsersOthersView(viewingUserId, onlineDetector);
    }
    
    public Task<KahlaUser?> GetUserByIdWithCacheAsync(string userId)
    {
        return cache.RunWithCache($"kahla-user-entity-{userId}", () =>
        {
            return relationalDbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == userId);
        });
    }
}