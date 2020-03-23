using Aiursoft.DocGenerator.Attributes;
using Aiursoft.Handler.Attributes;
using Aiursoft.Handler.Models;
using Aiursoft.Pylon;
using Aiursoft.Pylon.Attributes;
using Kahla.SDK.Models;
using Kahla.SDK.Models.ApiAddressModels;
using Kahla.SDK.Models.ApiViewModels;
using Kahla.SDK.Services;
using Kahla.Server.Data;
using Kahla.Server.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.Server.Controllers
{
    [LimitPerMin(40)]
    [APIExpHandler]
    [APIModelStateChecker]
    [AiurForceAuth(directlyReject: true)]
    public class FriendshipController : Controller
    {
        private readonly UserManager<KahlaUser> _userManager;
        private readonly KahlaDbContext _dbContext;
        private readonly KahlaPushService _pusher;
        private readonly OnlineJudger _onlineJudger;
        private static readonly object _obj = new object();

        public FriendshipController(
            UserManager<KahlaUser> userManager,
            KahlaDbContext dbContext,
            KahlaPushService pushService,
            OnlineJudger onlineJudger)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _pusher = pushService;
            _onlineJudger = onlineJudger;
        }

        [APIProduces(typeof(MineViewModel))]
        public async Task<IActionResult> Mine()
        {
            var user = await GetKahlaUser();
            var personalRelations = (await _dbContext.PrivateConversations
               .AsNoTracking()
               .Where(t => t.RequesterId == user.Id || t.TargetId == user.Id)
               .Select(t => user.Id == t.RequesterId ? t.TargetUser : t.RequestUser)
               .ToListAsync())
               .Select(t => t.Build(_onlineJudger))
               .OrderBy(t => t.NickName);
            var groups = await _dbContext.GroupConversations
                .AsNoTracking()
                .Where(t => t.Users.Any(p => p.UserId == user.Id))
                .OrderBy(t => t.GroupName)
                .ToListAsync();
            var searched = SearchedGroup.Map(groups);
            return Json(new MineViewModel
            {
                Code = ErrorType.Success,
                Message = "Successfully get all your groups and friends.",
                Users = personalRelations,
                Groups = searched,
            });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFriend([Required]string id)
        {
            var user = await GetKahlaUser();
            await _dbContext.Entry(user)
                .Collection(t => t.HisDevices)
                .LoadAsync();
            var target = await _dbContext.Users.Include(t => t.HisDevices).SingleOrDefaultAsync(t => t.Id == id);
            if (target == null)
            {
                return this.Protocol(ErrorType.NotFound, "We can not find target user.");
            }
            if (!await _dbContext.AreFriends(user.Id, target.Id))
            {
                return this.Protocol(ErrorType.NotEnoughResources, "He is not your friend at all.");
            }
            var deletedConversationId = await _dbContext.RemoveFriend(user.Id, target.Id);
            await _dbContext.SaveChangesAsync();
            await Task.WhenAll(
                _pusher.FriendDeletedEvent(target.CurrentChannel, target.HisDevices, user, deletedConversationId),
                _pusher.FriendDeletedEvent(user.CurrentChannel, user.HisDevices, user, deletedConversationId)
            );
            return this.Protocol(ErrorType.Success, "Successfully deleted your friend relationship.");
        }

        [HttpPost]
        [APIProduces(typeof(AiurValue<int>))]
        public async Task<IActionResult> CreateRequest([Required]string id)
        {
            var user = await GetKahlaUser();
            await _dbContext.Entry(user)
                .Collection(t => t.HisDevices)
                .LoadAsync();
            var target = await _dbContext.Users.Include(t => t.HisDevices).SingleOrDefaultAsync(t => t.Id == id);
            if (target == null)
            {
                return this.Protocol(ErrorType.NotFound, "We can not find your target user!");
            }
            if (target.Id == user.Id)
            {
                return this.Protocol(ErrorType.RequireAttention, "You can't request yourself!");
            }
            var areFriends = await _dbContext.AreFriends(user.Id, target.Id);
            if (areFriends)
            {
                return this.Protocol(ErrorType.HasDoneAlready, "You two are already friends!");
            }
            Request request;
            lock (_obj)
            {
                var pending = _dbContext.Requests
                    .Where(t =>
                        t.CreatorId == user.Id && t.TargetId == target.Id ||
                        t.CreatorId == target.Id && t.TargetId == user.Id)
                    .Any(t => !t.Completed);
                if (pending)
                {
                    return this.Protocol(ErrorType.HasDoneAlready, "There are some pending request hasn't been completed!");
                }
                request = new Request
                {
                    CreatorId = user.Id,
                    Creator = user,
                    TargetId = id,
                };
                _dbContext.Requests.Add(request);
                _dbContext.SaveChanges();
            }
            await Task.WhenAll(
                _pusher.NewFriendRequestEvent(target, request),
                _pusher.NewFriendRequestEvent(user, request)
            );
            return Json(new AiurValue<int>(request.Id)
            {
                Code = ErrorType.Success,
                Message = "Successfully created your request!"
            });
        }

        [HttpPost]
        public async Task<IActionResult> CompleteRequest(CompleteRequestAddressModel model)
        {
            var user = await GetKahlaUser();
            var request = await _dbContext
                .Requests
                .Include(t => t.Creator)
                .ThenInclude(t => t.HisDevices)
                .Include(t => t.Target)
                .ThenInclude(t => t.HisDevices)
                .SingleOrDefaultAsync(t => t.Id == model.Id);
            if (request == null)
            {
                return this.Protocol(ErrorType.NotFound, "We can not find target request.");
            }
            if (request.TargetId != user.Id)
            {
                return this.Protocol(ErrorType.Unauthorized, "The target user of this request is not you.");
            }
            if (request.Completed)
            {
                return this.Protocol(ErrorType.HasDoneAlready, "The target request is already completed.");
            }
            request.Completed = true;
            PrivateConversation newConversation = null;
            if (model.Accept)
            {
                if (await _dbContext.AreFriends(request.CreatorId, request.TargetId))
                {
                    await _dbContext.SaveChangesAsync();
                    return this.Protocol(ErrorType.RequireAttention, "You two are already friends.");
                }
                newConversation = _dbContext.AddFriend(request.CreatorId, request.TargetId);
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                await _dbContext.SaveChangesAsync();
            }
            await Task.WhenAll(
                _pusher.FriendsChangedEvent(
                    request.Creator,
                    request,
                    model.Accept,
                    newConversation.Build(request.CreatorId, _onlineJudger) as PrivateConversation),
                _pusher.FriendsChangedEvent(
                    request.Target,
                    request,
                    model.Accept,
                    newConversation.Build(request.TargetId, _onlineJudger) as PrivateConversation)
            );
            return this.Protocol(ErrorType.Success, "You have successfully completed this request.");
        }

        [APIProduces(typeof(AiurCollection<Request>))]
        public async Task<IActionResult> MyRequests()
        {
            var user = await GetKahlaUser();
            var requests = await _dbContext
                .Requests
                .AsNoTracking()
                .Include(t => t.Creator)
                .Where(t => t.TargetId == user.Id)
                .OrderByDescending(t => t.CreateTime)
                .ToListAsync();
            return Json(new AiurCollection<Request>(requests)
            {
                Code = ErrorType.Success,
                Message = "Successfully get your requests list."
            });
        }

        [APIProduces(typeof(SearchEverythingViewModel))]
        public async Task<IActionResult> SearchEverything(SearchEverythingAddressModel model)
        {
            var users = _dbContext
                .Users
                .AsNoTracking()
                .Where(t => t.ListInSearchResult || t.Id == model.SearchInput)
                .Where(t =>
                    t.MarkEmailPublic && t.Email.Contains(model.SearchInput) ||
                    t.NickName.Contains(model.SearchInput) ||
                    t.Id == model.SearchInput);

            var groups = _dbContext
                .GroupConversations
                .AsNoTracking()
                .Where(t => t.ListInSearchResult || t.Id.ToString() == model.SearchInput)
                .Where(t => t.GroupName.Contains(model.SearchInput));

            var searched = SearchedGroup.Map(await groups.ToListAsync());

            return Json(new SearchEverythingViewModel
            {
                UsersCount = await users.CountAsync(),
                GroupsCount = await groups.CountAsync(),
                Users = await users.Take(model.Take).ToListAsync(),
                Groups = searched,
                Code = ErrorType.Success,
                Message = "Search result is shown."
            });
        }

        [APIProduces(typeof(AiurCollection<FriendDiscovery>))]
        public async Task<IActionResult> DiscoverFriends(int take = 15)
        {
            // Load everything to memory and even functions.
            var users = await _dbContext.Users
                .AsNoTracking()
                .ToListAsync();
            var conversations = await _dbContext.PrivateConversations
                .AsNoTracking()
                .ToListAsync();
            var groups = await _dbContext.GroupConversations
                .Include(t => t.Users)
                .AsNoTracking()
                .ToListAsync();
            var requests = await _dbContext.Requests
                .AsNoTracking()
                .ToListAsync();
            bool AreFriends(string userId1, string userId2)
            {
                var relation = conversations.Any(t => t.RequesterId == userId1 && t.TargetId == userId2);
                if (relation)
                {
                    return true;
                }
                var elation = conversations.Any(t => t.RequesterId == userId2 && t.TargetId == userId1);
                return elation;
            }
            List<string> HisPersonalFriendsId(string userId)
            {
                var personalRelations = conversations
                    .Where(t => t.RequesterId == userId || t.TargetId == userId)
                    .Select(t => userId == t.RequesterId ? t.TargetId : t.RequesterId)
                    .ToList();
                return personalRelations;
            }
            List<int> HisGroups(string userId)
            {
                return groups
                    .Where(t => t.Users.Any(p => p.UserId == userId))
                    .Select(t => t.Id)
                    .ToList();
            }
            bool SentRequest(string userId1, string userId2)
            {
                var relation = requests.Where(t => t.Completed == false).Any(t => t.CreatorId == userId1 && t.TargetId == userId2);
                if (relation)
                {
                    return true;
                }
                var elation = requests.Where(t => t.Completed == false).Any(t => t.TargetId == userId1 && t.CreatorId == userId1);
                return elation;
            }
            var currentUser = await GetKahlaUser();
            var myFriends = HisPersonalFriendsId(currentUser.Id);
            var myGroups = HisGroups(currentUser.Id);
            var calculated = new List<FriendDiscovery>();
            foreach (var user in users)
            {
                if (user.Id == currentUser.Id || AreFriends(user.Id, currentUser.Id))
                {
                    continue;
                }
                var hisFriends = HisPersonalFriendsId(user.Id);
                var hisGroups = HisGroups(user.Id);
                var commonFriends = myFriends.Intersect(hisFriends).Count();
                var commonGroups = myGroups.Intersect(hisGroups).Count();
                if (commonFriends > 0 || commonGroups > 0)
                {
                    calculated.Add(new FriendDiscovery
                    {
                        CommonFriends = commonFriends,
                        CommonGroups = commonGroups,
                        TargetUser = user,
                        SentRequest = SentRequest(currentUser.Id, user.Id)
                    });
                }
            }
            var ordered = calculated
                .OrderByDescending(t => t.CommonFriends)
                .ThenBy(t => t.CommonGroups)
                .Take(take)
                .ToList();
            return Json(new AiurCollection<FriendDiscovery>(ordered)
            {
                Code = ErrorType.Success,
                Message = "Successfully get your suggested friends."
            });
        }

        [APIProduces(typeof(UserDetailViewModel))]
        public async Task<IActionResult> UserDetail([Required]string id)
        {
            var user = await GetKahlaUser();
            var target = await _dbContext.Users.AsNoTracking().SingleOrDefaultAsync(t => t.Id == id);
            var model = new UserDetailViewModel();
            if (target == null)
            {
                model.Message = "We can not find target user.";
                model.Code = ErrorType.NotFound;
                return Json(model);
            }
            var conversation = await _dbContext.FindConversationAsync(user.Id, target.Id);
            if (conversation != null)
            {
                model.AreFriends = true;
                model.ConversationId = conversation.Id;
            }
            else
            {
                model.AreFriends = false;
                model.ConversationId = null;
            }
            model.User = target.Build(_onlineJudger);
            model.PendingRequest = await _dbContext.Requests
                .Include(t => t.Creator)
                .Where(t =>
                    t.CreatorId == user.Id && t.TargetId == target.Id ||
                    t.CreatorId == target.Id && t.TargetId == user.Id)
                .FirstOrDefaultAsync(t => !t.Completed);
            model.SentRequest = model.PendingRequest != null;
            model.Message = "Found that user.";
            model.Code = ErrorType.Success;
            return Json(model);
        }

        [HttpPost]
        public async Task<IActionResult> ReportHim(ReportHimAddressModel model)
        {
            var currentUser = await GetKahlaUser();
            var targetUser = await _dbContext.Users.SingleOrDefaultAsync(t => t.Id == model.TargetUserId);
            if (targetUser == null)
            {
                return this.Protocol(ErrorType.NotFound, $"Could not find target user with id `{model.TargetUserId}`!");
            }
            if (currentUser.Id == targetUser.Id)
            {
                return this.Protocol(ErrorType.HasDoneAlready, "You can not report yourself!");
            }
            var exists = await _dbContext
                .Reports
                .AnyAsync((t) => t.TriggerId == currentUser.Id && t.TargetId == targetUser.Id && t.Status == ReportStatus.Pending);
            if (exists)
            {
                return this.Protocol(ErrorType.HasDoneAlready, "You have already reported the target user!");
            }
            // All check passed. Report him now!
            _dbContext.Reports.Add(new Report
            {
                TargetId = targetUser.Id,
                TriggerId = currentUser.Id,
                Reason = model.Reason
            });
            await _dbContext.SaveChangesAsync();
            return this.Protocol(ErrorType.Success, "Successfully reported target user!");
        }

        private Task<KahlaUser> GetKahlaUser() => _userManager.GetUserAsync(User);
    }
}
