using Aiursoft.AiurProtocol;
using Aiursoft.AiurProtocol.Models;
using Aiursoft.AiurProtocol.Services;
using Aiursoft.Kahla.SDK.Models.AddressModels;
using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.SDK.Models.ViewModels;
using Microsoft.Extensions.Options;

namespace Aiursoft.Kahla.SDK.Services;

public class KahlaServerAccess(
    AiurProtocolClient http,
    IOptions<KahlaServerConfig> demoServerLocator)
{
    private readonly KahlaServerConfig _demoServerLocator = demoServerLocator.Value;

    public async Task<IndexViewModel> ServerInfoAsync()
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/", param: new { });
        var result = await http.Get<IndexViewModel>(url);
        return result;
    }

    public async Task<AiurResponse> SignInAsync(string email, string password)
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/auth/signin", param: new { });
        var model = new AiurApiPayload(new SignInAddressModel
        {
            Email = email,
            Password = password
        });
        var result = await http.Post<AiurResponse>(url, model);
        return result;
    }

    public async Task<AiurResponse> RegisterAsync(string email, string password)
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/auth/register", param: new { });
        var model = new AiurApiPayload(new RegisterAddressModel
        {
            Email = email,
            Password = password
        });
        var result = await http.Post<AiurResponse>(url, model);
        return result;
    }

    public async Task<AiurResponse> ChangePasswordAsync(string oldPassword, string newPassword)
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/auth/change-password", param: new { });
        var model = new AiurApiPayload(new ChangePasswordAddressModel
        {
            OldPassword = oldPassword,
            NewPassword = newPassword
        });
        var result = await http.Post<AiurResponse>(url, model);
        return result;
    }

    public async Task<AiurResponse> SignoutAsync(int? deviceId = null)
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/auth/signout", param: new { });
        var model = new AiurApiPayload(new SignOutAddressModel
        {
            DeviceId = deviceId
        });
        var result = await http.Post<AiurResponse>(url, model);
        return result;
    }

    public async Task<MeViewModel> MeAsync()
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/auth/me", param: new { });
        var result = await http.Get<MeViewModel>(url);
        return result;
    }

    public async Task<AiurResponse> UpdateMeAsync(
        string? nickName = null,
        string? bio = null,
        int? themeId = null,
        bool? enableEmailNotification = null,
        bool? enableEnterToSendMessage = null,
        bool? enableHideMyOnlineStatus = null,
        bool? listInSearchResult = null,
        bool? allowHardInvitation = null)
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/auth/update-me", param: new { });
        var model = new AiurApiPayload(new UpdateMeAddressModel
        {
            NickName = nickName,
            Bio = bio,
            ThemeId = themeId,
            EnableEmailNotification = enableEmailNotification,
            EnableEnterToSendMessage = enableEnterToSendMessage,
            EnableHideMyOnlineStatus = enableHideMyOnlineStatus,
            AllowSearchByName = listInSearchResult,
            AllowHardInvitation = allowHardInvitation
        });
        var result = await http.Patch<MeViewModel>(url, model);
        return result;
    }

    public async Task<AiurCollection<DeviceMappedOwnerView>> MyDevicesAsync()
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/devices/my-devices", param: new { });
        var result = await http.Get<AiurCollection<DeviceMappedOwnerView>>(url);
        return result;
    }

    public async Task<AiurValue<int>> AddDeviceAsync(string name, string pushAuth, string pushEndpoint,
        string pushP256Dh)
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/devices/add-device", param: new { });
        var payload = new AiurApiPayload(new AddDeviceAddressModel
        {
            Name = name,
            PushAuth = pushAuth,
            PushEndpoint = pushEndpoint,
            PushP256Dh = pushP256Dh
        });
        var result = await http.Post<AiurValue<int>>(url, payload);
        return result;
    }

    public async Task<AiurResponse> DropDeviceAsync(int id)
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/devices/drop-device/{id}",
            param: new { id });
        var result = await http.Post<AiurResponse>(url, new AiurApiPayload(new { }));
        return result;
    }

    public async Task<AiurResponse> UpdateDeviceAsync(int id, string name, string pushAuth, string pushEndpoint,
        string pushP256Dh)
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/devices/update-device/{id}",
            param: new { id });
        var payload = new AiurApiPayload(new AddDeviceAddressModel
        {
            Name = name,
            PushAuth = pushAuth,
            PushEndpoint = pushEndpoint,
            PushP256Dh = pushP256Dh
        });
        var result = await http.Put<AiurResponse>(url, payload);
        return result;
    }

    public async Task<AiurResponse> PushTestAsync()
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/devices/push-test-message",
            param: new { });
        var result = await http.Post<AiurResponse>(url, new AiurApiPayload(new { }));
        return result;
    }

    public async Task<InitPusherViewModel> InitPusherAsync()
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/messages/init-websocket",
            param: new { });
        var result = await http.Post<InitPusherViewModel>(url, new AiurApiPayload(new { }));
        return result;
    }

    public async Task<InitPusherViewModel> InitThreadWebSocketAsync(int threadId)
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance,
            route: "/api/messages/init-thread-websocket/{threadId}", param:
            new { threadId });
        var result = await http.Post<InitPusherViewModel>(url, new AiurApiPayload(new { }));
        return result;
    }

    public async Task<SearchUsersViewModel> SearchUsersGloballyAsync(string? searchInput, string? excluding = null,
        int skip = 0, int take = 20)
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/search/search-users",
            param: new SearchAddressModel
            {
                SearchInput = searchInput,
                Excluding = excluding,
                Skip = skip,
                Take = take
            });
        var result = await http.Get<SearchUsersViewModel>(url);
        return result;
    }

    public async Task<SearchThreadsViewModel> SearchThreadsGloballyAsync(string? searchInput, string? excluding = null,
        int skip = 0, int take = 20)
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/search/search-threads",
            param: new SearchAddressModel
            {
                SearchInput = searchInput,
                Excluding = excluding,
                Skip = skip,
                Take = take
            });
        var result = await http.Get<SearchThreadsViewModel>(url);
        return result;
    }

    public async Task<MyContactsViewModel> ListContactsAsync(string? search = null, string? excluding = null,
        int skip = 0, int take = 20)
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/contacts/list",
            param: new SearchAddressModel
            {
                SearchInput = search,
                Excluding = excluding,
                Skip = skip,
                Take = take
            });
        var result = await http.Get<MyContactsViewModel>(url);
        return result;
    }

    public async Task<UserBriefViewModel> UserBriefAsync(string userId, int skip = 0, int take = 20)
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/contacts/info/{userId}", param: new
        {
            userId, skip, take
        });
        var result = await http.Get<UserBriefViewModel>(url);
        return result;
    }

    public async Task<UserDetailViewModel> UserDetailAsync(string userId, int skip = 0, int take = 20)
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/contacts/details/{userId}", param: new
        {
            userId, skip, take
        });
        var result = await http.Get<UserDetailViewModel>(url);
        return result;
    }

    public async Task<AiurResponse> AddContactAsync(string userId)
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/contacts/add/{userId}",
            param: new { userId });
        var result = await http.Post<AiurResponse>(url, new AiurApiPayload(new { }));
        return result;
    }

    public async Task<AiurResponse> RemoveContactAsync(string userId)
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/contacts/remove/{userId}",
            param: new { userId });
        var result = await http.Post<AiurResponse>(url, new AiurApiPayload(new { }));
        return result;
    }

    public async Task<AiurResponse> ReportUserAsync(string userId, string reason)
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/contacts/report/", param: new { });
        var result = await http.Post<AiurResponse>(url, new AiurApiPayload(new ReportHimAddressModel
        {
            TargetUserId = userId,
            Reason = reason
        }));
        return result;
    }

    public async Task<MyBlocksViewModel> ListBlocksAsync(
        string? search = null, 
        string? excluding = null, 
        int skip = 0,
        int take = 20)
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/blocks/list",
            param: new SearchAddressModel
            {
                SearchInput = search,
                Excluding = excluding,
                Skip = skip,
                Take = take
            });
        var result = await http.Get<MyBlocksViewModel>(url);
        return result;
    }

    public async Task<AiurResponse> BlockNewAsync(string userId)
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/blocks/block/{userId}",
            param: new { userId });
        var result = await http.Post<AiurResponse>(url, new AiurApiPayload(new { }));
        return result;
    }

    public async Task<AiurResponse> UnblockAsync(string userId)
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/blocks/remove/{userId}",
            param: new { userId });
        var result = await http.Post<AiurResponse>(url, new AiurApiPayload(new { }));
        return result;
    }

    public async Task<MyThreadsViewModel> MyThreadsAsync(int? skipTillThreadId = null, int take = 20)
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/threads/mine",
            param: new MyThreadsAddressModel
            {
                SkipTillThreadId = skipTillThreadId,
                Take = take
            });
        var result = await http.Get<MyThreadsViewModel>(url);
        return result;
    }
    
    public async Task<MyThreadsViewModel> SearchThreadsAsync(
        string? search = null, 
        string? excluding = null,
        int skip = 0, 
        int take = 20)
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/threads/search",
            param: new SearchAddressModel
            {
                SearchInput = search,
                Excluding = excluding,
                Skip = skip,
                Take = take
            });
        var result = await http.Get<MyThreadsViewModel>(url);
        return result;
    }

    public async Task<ThreadMembersViewModel> ThreadMembersAsync(
        int id,
        string? search = null,
        string? excluding = null,
        int skip = 0, 
        int take = 20)
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: $"/api/threads/members/{id}",
            param: new SearchAddressModel
            {
                SearchInput = search,
                Excluding = excluding,
                Skip = skip,
                Take = take
            });
        var result = await http.Get<ThreadMembersViewModel>(url);
        return result;
    }

    public async Task<ThreadAnonymousViewModel> ThreadDetailsAnonymousAsync(int id)
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/threads/details-anonymous/{id}",
            param: new { id });
        var result = await http.Get<ThreadAnonymousViewModel>(url);
        return result;
    }

    public async Task<ThreadDetailsViewModel> ThreadDetailsJoinedAsync(int id)
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/threads/details-joined/{id}",
            param: new { id });
        var result = await http.Get<ThreadDetailsViewModel>(url);
        return result;
    }

    public async Task<AiurResponse> UpdateThreadAsync(
        int id, 
        string? name = null, 
        string? iconFilePath = null,
        bool? allowDirectJoinWithoutInvitation = null, 
        bool? allowMemberSoftInvitation = null,
        bool? allowMembersSendMessages = null, 
        bool? allowMembersEnlistAllMembers = null,
        bool? allowSearchByName = null)
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/threads/update-thread/{id}",
            param: new { id });
        var model = new AiurApiPayload(new UpdateThreadAddressModel
        {
            Name = name,
            IconFilePath = iconFilePath,
            AllowDirectJoinWithoutInvitation = allowDirectJoinWithoutInvitation,
            AllowMemberSoftInvitation = allowMemberSoftInvitation,
            AllowMembersSendMessages = allowMembersSendMessages,
            AllowMembersEnlistAllMembers = allowMembersEnlistAllMembers,
            AllowSearchByName = allowSearchByName
        });
        var result = await http.Patch<AiurResponse>(url, model);
        return result;
    }

    public async Task<AiurResponse> DirectJoinAsync(int id)
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/threads/direct-join/{id}",
            param: new { id });
        var result = await http.Post<AiurResponse>(url, new AiurApiPayload(new { }));
        return result;
    }

    public async Task<AiurResponse> TransferOwnershipAsync(int id, string targetUserId)
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/threads/transfer-ownership/{id}",
            param: new { id });
        var model = new AiurApiPayload(new
        {
            targetUserId
        });
        var result = await http.Post<AiurResponse>(url, model);
        return result;
    }

    public async Task<AiurResponse> PromoteAdminAsync(int id, string targetUserId, bool promote)
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/threads/promote-admin/{id}",
            param: new { id });
        var model = new AiurApiPayload(new
        {
            targetUserId,
            promote
        });
        var result = await http.Post<AiurResponse>(url, model);
        return result;
    }

    public async Task<AiurResponse> KickMemberAsync(int id, string targetUserId)
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/threads/kick-member/{id}",
            param: new { id });
        var model = new AiurApiPayload(new
        {
            targetUserId
        });
        var result = await http.Post<AiurResponse>(url, model);
        return result;
    }

    public async Task<AiurResponse> LeaveThreadAsync(int id)
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/threads/leave/{id}", param: new { id });
        var result = await http.Post<AiurResponse>(url, new AiurApiPayload(new { }));
        return result;
    }

    public async Task<AiurResponse> DissolveThreadAsync(int id)
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/threads/dissolve/{id}",
            param: new { id });
        var result = await http.Post<AiurResponse>(url, new AiurApiPayload(new { }));
        return result;
    }

    public async Task<AiurResponse> SetMuteAsync(int id, bool mute)
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/threads/set-mute/{id}",
            param: new { id });
        var model = new AiurApiPayload(new
        {
            mute
        });
        var result = await http.Post<AiurResponse>(url, model);
        return result;
    }

    public async Task<CreateNewThreadViewModel> CreateFromScratchAsync(string name, bool allowSearchByName,
        bool allowDirectJoinWithoutInvitation, bool allowMemberSoftInvitation, bool allowMembersSendMessages,
        bool allowMembersEnlistAllMembers)
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/threads/create-scratch",
            param: new { });
        var model = new AiurApiPayload(new CreateThreadAddressModel
        {
            Name = name,
            AllowSearchByName = allowSearchByName,
            AllowDirectJoinWithoutInvitation = allowDirectJoinWithoutInvitation,
            AllowMemberSoftInvitation = allowMemberSoftInvitation,
            AllowMembersSendMessages = allowMembersSendMessages,
            AllowMembersEnlistAllMembers = allowMembersEnlistAllMembers
        });
        var result = await http.Post<CreateNewThreadViewModel>(url, model);
        return result;
    }

    public async Task<CreateNewThreadViewModel> HardInviteAsync(string id)
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/threads/hard-invite/{id}",
            param: new { id });
        var result = await http.Post<CreateNewThreadViewModel>(url, new AiurApiPayload(new { }));
        return result;
    }

    public async Task<CreateSoftInviteTokenViewModel> CreateSoftInviteTokenAsync(int id, string invitedUserId)
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/threads/soft-invite-init/{id}",
            param: new { id });
        var model = new AiurApiPayload(new
        {
            InvitedUserId = invitedUserId
        });
        var result = await http.Post<CreateSoftInviteTokenViewModel>(url, model);
        return result;
    }

    public async Task<AiurResponse> CompleteSoftInviteAsync(string token)
    {
        var url = new AiurApiEndpoint(_demoServerLocator.Instance, route: "/api/threads/soft-invite-complete",
            param: new { });
        var model = new AiurApiPayload(new
        {
            Token = token
        });
        var result = await http.Post<AiurResponse>(url, model);
        return result;
    }
}