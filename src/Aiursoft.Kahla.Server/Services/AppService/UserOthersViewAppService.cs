using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.Server.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Services.AppService;

public class UserOthersViewAppService(
    OnlineJudger judger,
    UserOthersViewRepo repo)
{
    public async Task<(int totalCount, List<KahlaUserMappedOthersView> contacts)> GetMyContactsPagedAsync(string viewingUserId, int skip, int take)
    {
        var query = repo.QueryMyContacts(viewingUserId);
        var totalCount = await query.CountAsync();
        var contacts = await query.Skip(skip).Take(take).ToListAsync();
        contacts.ForEach(t => t.Online = judger.IsOnline(t.User.Id, t.User.EnableHideMyOnlineStatus));
        return (totalCount, contacts);
    }
        
    public async Task<(int totalCount, List<KahlaUserMappedOthersView> blocks)> GetMyBlocksPagedAsync(string viewingUserId, int skip, int take)
    {
        var query = repo.QueryMyBlocksPaged(viewingUserId);
        var totalCount = await query.CountAsync();
        var blocks = await query.Skip(skip).Take(take).ToListAsync();
        blocks.ForEach(t => t.Online = judger.IsOnline(t.User.Id, t.User.EnableHideMyOnlineStatus));
        return (totalCount, blocks);
    }

    public async Task<(int totalCount, List<KahlaUserMappedOthersView> users)> SearchUsersPagedAsync(string searchInput, string viewingUserId, int skip, int take)
    {
        var query = repo.SearchUsers(searchInput, viewingUserId);
        var totalCount = await query.CountAsync();
        var users = await query.Skip(skip).Take(take).ToListAsync();
        users.ForEach(t => t.Online = judger.IsOnline(t.User.Id, t.User.EnableHideMyOnlineStatus));
        return (totalCount, users);
    }
    
    public async Task<KahlaUserMappedOthersView?> GetUserByIdAsync(string targetUserId, string viewingUserId)
    {
        var user = await repo.QueryUserById(
                targetUserId: targetUserId,
                viewingUserId: viewingUserId)
            .FirstOrDefaultAsync();
        if (user != null)
        {
            user.Online = judger.IsOnline(user.User.Id, user.User.EnableHideMyOnlineStatus);
        }
        return user;
    }
}