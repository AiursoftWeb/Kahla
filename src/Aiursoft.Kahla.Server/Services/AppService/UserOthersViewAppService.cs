using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.Server.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Services.AppService;

public class UserOthersViewAppService(
    UserOthersViewRepo repo)
{
    public async Task<(int totalCount, List<KahlaUserMappedOthersView> contacts)> GetMyContactsPagedAsync(string viewingUserId, int skip, int take)
    {
        var query = repo.QueryMyContacts(viewingUserId);
        var totalCount = await query.CountAsync();
        var contacts = await query.Skip(skip).Take(take).ToListAsync();
        return (totalCount, contacts);
    }
    
    public async Task<(int totalCount, List<KahlaUserMappedOthersView> contacts)> SearchMyContactsPagedAsync(string searchInput, string viewingUserId, int skip, int take)
    {
        var query = repo.SearchMyContactsAsync(searchInput, viewingUserId);
        var totalCount = await query.CountAsync();
        var contacts = await query.Skip(skip).Take(take).ToListAsync();
        return (totalCount, contacts);
    }
        
    public async Task<(int totalCount, List<KahlaUserMappedOthersView> blocks)> GetMyBlocksPagedAsync(string viewingUserId, int skip, int take)
    {
        var query = repo.QueryMyBlocksPaged(viewingUserId);
        var totalCount = await query.CountAsync();
        var blocks = await query.Skip(skip).Take(take).ToListAsync();
        return (totalCount, blocks);
    }

    public async Task<(int totalCount, List<KahlaUserMappedOthersView> blocks)> SearchMyBlocksPagedAsync(string searchInput, string viewingUserId, int skip, int take)
    {
        var query = repo.SearchMyBlocksAsync(searchInput, viewingUserId);
        var totalCount = await query.CountAsync();
        var blocks = await query.Skip(skip).Take(take).ToListAsync();
        return (totalCount, blocks);
    }

    public async Task<(int totalCount, List<KahlaUserMappedOthersView> users)> SearchUsersPagedAsync(string searchInput, string viewingUserId, int skip, int take)
    {
        var query = repo.SearchUsers(searchInput, viewingUserId);
        var totalCount = await query.CountAsync();
        var users = await query.Skip(skip).Take(take).ToListAsync();
        return (totalCount, users);
    }
    
    public async Task<KahlaUserMappedOthersView?> GetUserByIdAsync(string targetUserId, string viewingUserId)
    {
        var user = await repo.QueryUserById(
                targetUserId: targetUserId,
                viewingUserId: viewingUserId)
            .FirstOrDefaultAsync();
        return user;
    }
}