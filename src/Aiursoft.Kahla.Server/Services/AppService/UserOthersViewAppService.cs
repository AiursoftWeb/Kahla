using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.Server.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Services.AppService;

public class UserOthersViewAppService(
    OnlineJudger judger,
    UserOthersViewRepo repo)
{
    public async Task<List<KahlaUserMappedOthersView>> GetMyContactsPagedAsync(string viewingUserId, int take)
    {
        var views = await repo.QueryMyContacts(viewingUserId, take).ToListAsync();
        views.ForEach(t => t.Online = judger.IsOnline(t.User.Id, t.User.EnableHideMyOnlineStatus));
        return views;
    }
        
    public async Task<List<KahlaUserMappedOthersView>> GetMyBlocksPagedAsync(string viewingUserId, int take)
    {
        var views = await repo.QueryMyBlocksPaged(viewingUserId, take).ToListAsync();
        views.ForEach(t => t.Online = judger.IsOnline(t.User.Id, t.User.EnableHideMyOnlineStatus));
        return views;
    }

    public async Task<(int count, List<KahlaUserMappedOthersView> users)> SearchUsersPagedAsync(string searchInput, string viewingUserId, int take)
    {
        var query = repo.SearchUsers(searchInput, viewingUserId);
        var totalCount = await query.CountAsync();
        var users = await query.Take(take).ToListAsync();
        users.ForEach(t => t.Online = judger.IsOnline(t.User.Id, t.User.EnableHideMyOnlineStatus));
        return (totalCount, users);
    }
    
    public async Task<KahlaUserMappedOthersView?> GetUserById(string targetUserId, string viewingUserId)
    {
        var user = await repo.QueryUserById(
                targetUserId: targetUserId,
                viewingUserId: viewingUserId)
            .FirstOrDefaultAsync();

        return user;
    }
}