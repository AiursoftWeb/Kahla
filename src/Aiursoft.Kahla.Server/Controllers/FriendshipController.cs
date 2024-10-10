using System.ComponentModel.DataAnnotations;
using Aiursoft.AiurProtocol.Exceptions;
using Aiursoft.AiurProtocol.Models;
using Aiursoft.AiurProtocol.Server;
using Aiursoft.AiurProtocol.Server.Attributes;
using Aiursoft.DocGenerator.Attributes;
using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Models.Conversations;
using Aiursoft.Kahla.Server.Attributes;
using Aiursoft.Kahla.Server.Data;
using Aiursoft.Kahla.Server.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Controllers;

[KahlaForceAuth]
[Obsolete]
[GenerateDoc]
[ApiExceptionHandler(
    PassthroughRemoteErrors = true,
    PassthroughAiurServerException = true)]
[ApiModelStateChecker]
[Route("api/friendship")]
public class FriendshipController(
    IConfiguration configuration,
    KahlaPushService pusher,
    UserManager<KahlaUser> userManager,
    KahlaDbContext dbContext)
    : ControllerBase
{
    private static readonly SemaphoreSlim CreateRequestLock = new(1, 1);
    private static readonly SemaphoreSlim AcceptRequestLock = new(1, 1);

    [HttpPost]
    [Route("delete-friend/{id}")]
    public async Task<IActionResult> DeleteFriend([Required][FromRoute] string id)
    {
        var user = await this.GetCurrentUser(userManager);
        await dbContext.Entry(user)
            .Collection(t => t.HisDevices)
            .LoadAsync();
        var target = await dbContext.Users.Include(t => t.HisDevices).SingleOrDefaultAsync(t => t.Id == id);
        if (target == null)
        {
            return this.Protocol(Code.NotFound, "We can not find target user.");
        }
        if (!await dbContext.AreFriends(user.Id, target.Id))
        {
            return this.Protocol(Code.NotFound, "He is not your friend at all.");
        }
        var deletedConversationId = await dbContext.RemoveFriend(user.Id, target.Id);
        await dbContext.SaveChangesAsync();
        pusher.FriendDeletedEvent(target, user, deletedConversationId);
        pusher.FriendDeletedEvent(user, user, deletedConversationId);
        // TODO: Delete the folder which saves the files of the conversation.
        return this.Protocol(Code.JobDone, "Successfully deleted your friend relationship.");
    }
    
    [HttpPost]
    [Route("create-request/{id}")]
    public async Task<IActionResult> CreateRequest([Required][FromRoute] string id)
    {
        var user = await this.GetCurrentUser(userManager);
        await dbContext.Entry(user)
            .Collection(t => t.HisDevices)
            .LoadAsync();
        var target = await dbContext.Users.Include(t => t.HisDevices).SingleOrDefaultAsync(t => t.Id == id);
        if (target == null)
        {
            return this.Protocol(Code.NotFound, "We can not find your target user!");
        }
        if (target.Id == user.Id)
        {
            return this.Protocol(Code.Conflict, "You can't request yourself!");
        }
        var areFriends = await dbContext.AreFriends(user.Id, target.Id);
        if (areFriends)
        {
            return this.Protocol(Code.Conflict, "You two are already friends!");
        }
        Request request;
        await CreateRequestLock.WaitAsync();
        try
        {
            var pending = await dbContext.Requests
                .Where(t =>
                    t.CreatorId == user.Id && t.TargetId == target.Id ||
                    t.CreatorId == target.Id && t.TargetId == user.Id)
                .AnyAsync(t => !t.Completed);
            if (pending)
            {
                return this.Protocol(Code.Conflict, "There are some pending request hasn't been completed!");
            }
            request = new Request
            {
                CreatorId = user.Id,
                Creator = user,
                TargetId = id,
                Target = target,
            };
            await dbContext.Requests.AddAsync(request);
            await dbContext.SaveChangesAsync();
        }
        finally
        {
            CreateRequestLock.Release();
        }

        pusher.NewFriendRequestEvent(target, request);
        pusher.NewFriendRequestEvent(user, request);
        if (configuration["AutoAcceptRequests"] == true.ToString().ToLower())
        {
            await AcceptRequest(request, true);
        }
        return this.Protocol(new AiurValue<int>(request.Id)
        {
            Code = Code.JobDone,
            Message = "Successfully created your request!"
        });
    }
    //
    // [HttpPost]
    // [Produces(typeof(AiurValue<int>))]
    // public async Task<IActionResult> CompleteRequest(CompleteRequestAddressModel model)
    // {
    //     var user = await GetKahlaUser();
    //     var request = await _dbContext
    //         .Requests
    //         .Include(t => t.Creator)
    //         .ThenInclude(t => t.HisDevices)
    //         .Include(t => t.Target)
    //         .ThenInclude(t => t.HisDevices)
    //         .SingleOrDefaultAsync(t => t.Id == model.Id);
    //     if (request == null)
    //     {
    //         return this.Protocol(Code.NotFound, "We can not find target request.");
    //     }
    //     if (request.TargetId != user.Id)
    //     {
    //         return this.Protocol(Code.Unauthorized, "The target user of this request is not you.");
    //     }
    //     if (request.Completed)
    //     {
    //         var conversation = _dbContext.FindConversationAsync(request.TargetId, request.CreatorId);
    //         if (conversation != null)
    //         {
    //             return this.Protocol(new AiurValue<int?>(conversation.Id)
    //             {
    //                 Code = Code.NoActionTaken,
    //                 Message = $"You have already completed this request and the conversation with ID: '{conversation.Id}' still exists."
    //             });
    //         }
    //         return this.Protocol(new AiurValue<int?>(null)
    //         {
    //             Code = Code.NoActionTaken,
    //             Message = "You have already completed this request. Created conversation was deleted."
    //         });
    //     }
    //     var newConversation = await AcceptRequest(request, model.Accept);
    //     return this.Protocol(new AiurValue<int?>(newConversation?.Id)
    //     {
    //         Code = Code.JobDone,
    //         Message = "You have successfully completed this request."
    //     });
    // }
    //
    // [Produces(typeof(AiurCollection<Request>))]
    // public async Task<IActionResult> MyRequests()
    // {
    //     var user = await GetKahlaUser();
    //     var requests = await _dbContext
    //         .Requests
    //         .AsNoTracking()
    //         .Include(t => t.Creator)
    //         .Where(t => t.TargetId == user.Id)
    //         .OrderByDescending(t => t.CreateTime)
    //         .ToListAsync();
    //     return this.Protocol(new AiurCollection<Request>(requests)
    //     {
    //         Code = Code.ResultShown,
    //         Message = "Successfully get your requests list."
    //     });
    // }
    //
    // [Produces(typeof(SearchEverythingViewModel))]
    // public async Task<IActionResult> SearchEverything(SearchEverythingAddressModel model)
    // {
    //     var users = _dbContext
    //         .Users
    //         .AsNoTracking()
    //         .Where(t => t.ListInSearchResult || t.Id == model.SearchInput)
    //         .Where(t =>
    //             t.MarkEmailPublic && t.Email.Contains(model.SearchInput) ||
    //             t.NickName.Contains(model.SearchInput) ||
    //             t.Id == model.SearchInput);
    //
    //     var groups = _dbContext
    //         .GroupConversations
    //         .AsNoTracking()
    //         .Where(t => t.ListInSearchResult || t.Id.ToString() == model.SearchInput)
    //         .Where(t => t.GroupName.Contains(model.SearchInput));
    //
    //     var searched = SearchedGroup.Map(await groups.ToListAsync());
    //
    //     return this.Protocol(new SearchEverythingViewModel
    //     {
    //         UsersCount = await users.CountAsync(),
    //         GroupsCount = await groups.CountAsync(),
    //         Users = await users.Take(model.Take).ToListAsync(),
    //         Groups = searched,
    //         Code = Code.ResultShown,
    //         Message = "Search result is shown."
    //     });
    // }
    //
    // [Produces(typeof(AiurCollection<FriendDiscovery>))]
    // public async Task<IActionResult> DiscoverFriends(int take = 15)
    // {
    //     // Load everything to memory and even functions.
    //     var users = await _dbContext.Users
    //         .AsNoTracking()
    //         .ToListAsync();
    //     var conversations = await _dbContext.PrivateConversations
    //         .AsNoTracking()
    //         .ToListAsync();
    //     var groups = await _dbContext.GroupConversations
    //         .Include(t => t.Users)
    //         .AsNoTracking()
    //         .ToListAsync();
    //     var requests = await _dbContext.Requests
    //         .AsNoTracking()
    //         .ToListAsync();
    //     bool AreFriends(string userId1, string userId2)
    //     {
    //         var relation = conversations.Any(t => t.RequesterId == userId1 && t.TargetId == userId2);
    //         if (relation)
    //         {
    //             return true;
    //         }
    //         var elation = conversations.Any(t => t.RequesterId == userId2 && t.TargetId == userId1);
    //         return elation;
    //     }
    //     List<string> HisPersonalFriendsId(string userId)
    //     {
    //         var personalRelations = conversations
    //             .Where(t => t.RequesterId == userId || t.TargetId == userId)
    //             .Select(t => userId == t.RequesterId ? t.TargetId : t.RequesterId)
    //             .ToList();
    //         return personalRelations;
    //     }
    //     List<int> HisGroups(string userId)
    //     {
    //         return groups
    //             .Where(t => t.Users.Any(p => p.UserId == userId))
    //             .Select(t => t.Id)
    //             .ToList();
    //     }
    //     bool SentRequest(string userId1, string userId2)
    //     {
    //         var relation = requests.Where(t => t.Completed == false).Any(t => t.CreatorId == userId1 && t.TargetId == userId2);
    //         if (relation)
    //         {
    //             return true;
    //         }
    //         var elation = requests.Where(t => t.Completed == false).Any(t => t.TargetId == userId1 && t.CreatorId == userId1);
    //         return elation;
    //     }
    //     var currentUser = await GetKahlaUser();
    //     var myFriends = HisPersonalFriendsId(currentUser.Id);
    //     var myGroups = HisGroups(currentUser.Id);
    //     var calculated = new List<FriendDiscovery>();
    //     foreach (var user in users)
    //     {
    //         if (user.Id == currentUser.Id || AreFriends(user.Id, currentUser.Id))
    //         {
    //             continue;
    //         }
    //         var hisFriends = HisPersonalFriendsId(user.Id);
    //         var hisGroups = HisGroups(user.Id);
    //         var commonFriends = myFriends.Intersect(hisFriends).Count();
    //         var commonGroups = myGroups.Intersect(hisGroups).Count();
    //         if (commonFriends > 0 || commonGroups > 0)
    //         {
    //             calculated.Add(new FriendDiscovery
    //             {
    //                 CommonFriends = commonFriends,
    //                 CommonGroups = commonGroups,
    //                 TargetUser = user,
    //                 SentRequest = SentRequest(currentUser.Id, user.Id)
    //             });
    //         }
    //     }
    //     var ordered = calculated
    //         .OrderByDescending(t => t.CommonFriends)
    //         .ThenBy(t => t.CommonGroups)
    //         .Take(take)
    //         .ToList();
    //     return this.Protocol(new AiurCollection<FriendDiscovery>(ordered)
    //     {
    //         Code = Code.ResultShown,
    //         Message = "Successfully get your suggested friends."
    //     });
    // }
    //
    // [Produces(typeof(UserDetailViewModel))]
    // public async Task<IActionResult> UserDetail([Required] string id)
    // {
    //     var user = await GetKahlaUser();
    //     var target = await _dbContext.Users.AsNoTracking().SingleOrDefaultAsync(t => t.Id == id);
    //     var model = new UserDetailViewModel();
    //     if (target == null)
    //     {
    //         model.Message = "We can not find target user.";
    //         model.Code = Code.NotFound;
    //         return this.Protocol(model);
    //     }
    //     var conversation = await _dbContext.FindConversationAsync(user.Id, target.Id);
    //     if (conversation != null)
    //     {
    //         model.AreFriends = true;
    //         model.ConversationId = conversation.Id;
    //     }
    //     else
    //     {
    //         model.AreFriends = false;
    //         model.ConversationId = null;
    //     }
    //     model.User = target.Build(_onlineJudger);
    //     model.PendingRequest = await _dbContext.Requests
    //         .Include(t => t.Creator)
    //         .Where(t =>
    //             t.CreatorId == user.Id && t.TargetId == target.Id ||
    //             t.CreatorId == target.Id && t.TargetId == user.Id)
    //         .FirstOrDefaultAsync(t => !t.Completed);
    //     model.SentRequest = model.PendingRequest != null;
    //     model.Message = "Found that user.";
    //     model.Code = Code.ResultShown;
    //     return this.Protocol(model);
    // }
    //
    // [HttpPost]
    // public async Task<IActionResult> ReportHim(ReportHimAddressModel model)
    // {
    //     var currentUser = await GetKahlaUser();
    //     var targetUser = await _dbContext.Users.SingleOrDefaultAsync(t => t.Id == model.TargetUserId);
    //     if (targetUser == null)
    //     {
    //         return this.Protocol(Code.NotFound, $"Could not find target user with id `{model.TargetUserId}`!");
    //     }
    //     if (currentUser.Id == targetUser.Id)
    //     {
    //         return this.Protocol(Code.Conflict, "You can not report yourself!");
    //     }
    //     var exists = await _dbContext
    //         .Reports
    //         .AnyAsync((t) => t.TriggerId == currentUser.Id && t.TargetId == targetUser.Id && t.Status == ReportStatus.Pending);
    //     if (exists)
    //     {
    //         return this.Protocol(Code.NoActionTaken, "You have already reported the target user!");
    //     }
    //     // All check passed. Report him now!
    //     await _dbContext.Reports.AddAsync(new Report
    //     {
    //         TargetId = targetUser.Id,
    //         TriggerId = currentUser.Id,
    //         Reason = model.Reason
    //     });
    //     await _dbContext.SaveChangesAsync();
    //     return this.Protocol(Code.JobDone, "Successfully reported target user!");
    // }
    //
    // private Task<KahlaUser> GetKahlaUser() => _userManager.GetUserAsync(User);
    //
    private async Task AcceptRequest(Request request, bool accept)
    {
        PrivateConversation? newConversation = null;
        request.Completed = true;
        await AcceptRequestLock.WaitAsync();
        try
        {
            if (accept)
            {
                if (await dbContext.AreFriends(request.CreatorId, request.TargetId))
                {
                    await dbContext.SaveChangesAsync();
                    throw new AiurServerException(Code.NoActionTaken, "You two are already friends.");
                }
                newConversation = dbContext.AddFriend(request.CreatorId, request.TargetId);
            }
            await dbContext.SaveChangesAsync();
        }
        finally
        {
            AcceptRequestLock.Release();
        }

        pusher.FriendRequestCompletedEvent(
            request.Creator ?? await dbContext.Users.FindAsync(request.CreatorId) ?? throw new AiurServerException(Code.NotFound, "Can not find the creator of this request!"),
            request,
            accept,
            newConversation);
        pusher.FriendRequestCompletedEvent(
            request.Target ?? await dbContext.Users.FindAsync(request.TargetId) ?? throw new AiurServerException(Code.NotFound, "Can not find the target of this request!"),
            request,
            accept,
            newConversation);
    }
}