using Aiursoft.Pylon;
using Aiursoft.Pylon.Attributes;
using Aiursoft.Pylon.Models;
using Kahla.Server.Data;
using Kahla.Server.Models;
using Kahla.Server.Models.ApiAddressModels;
using Kahla.Server.Models.ApiViewModels;
using Kahla.Server.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.Server.Controllers
{
    [APIExpHandler]
    [APIModelStateChecker]
    [AiurForceAuth(directlyReject: true)]
    public class FriendshipController : Controller
    {
        private readonly UserManager<KahlaUser> _userManager;
        private readonly KahlaDbContext _dbContext;
        private readonly KahlaPushService _pusher;
        private static readonly object Obj = new object();

        public FriendshipController(
            UserManager<KahlaUser> userManager,
            KahlaDbContext dbContext,
            KahlaPushService pushService)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _pusher = pushService;
        }

        public async Task<IActionResult> MyFriends([Required]bool? orderByName)
        {
            var user = await GetKahlaUser();
            var list = new List<ContactInfo>();
            var conversations = await _dbContext.MyConversations(user.Id);
            foreach (var conversation in conversations)
            {
                list.Add(new ContactInfo
                {
                    ConversationId = conversation.Id,
                    DisplayName = conversation.GetDisplayName(user.Id),
                    DisplayImageKey = conversation.GetDisplayImage(user.Id),
                    LatestMessage = conversation.GetLatestMessage().Content,
                    LatestMessageTime = conversation.GetLatestMessage().SendTime,
                    UnReadAmount = conversation.GetUnReadAmount(user.Id),
                    Discriminator = conversation.Discriminator,
                    UserId = (conversation as PrivateConversation)?.AnotherUser(user.Id).Id,
                    AesKey = conversation.AESKey,
                    Muted = conversation is GroupConversation && (await _dbContext.GetRelationFromGroup(user.Id, conversation.Id)).Muted
                });
            }
            list = orderByName == true ?
                list.OrderBy(t => t.DisplayName).ToList() :
                list.OrderByDescending(t => t.LatestMessageTime).ToList();
            return this.AiurJson(new AiurCollection<ContactInfo>(list)
            {
                Code = ErrorType.Success,
                Message = "Successfully get all your friends."
            });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFriend([Required]string id)
        {
            var user = await GetKahlaUser();
            var target = await _dbContext.Users.FindAsync(id);
            if (target == null)
                return this.Protocol(ErrorType.NotFound, "We can not find target user.");
            if (!await _dbContext.AreFriends(user.Id, target.Id))
                return this.Protocol(ErrorType.NotEnoughResources, "He is not your friend at all.");
            await _dbContext.RemoveFriend(user.Id, target.Id);
            await _dbContext.SaveChangesAsync();
            await _pusher.WereDeletedEvent(target.Id);
            return this.Protocol(ErrorType.Success, "Successfully deleted your friend relationship.");
        }

        [HttpPost]
        public async Task<IActionResult> CreateRequest([Required]string id)
        {
            var user = await GetKahlaUser();
            if (!user.EmailConfirmed)
                return this.Protocol(ErrorType.Unauthorized, "You are not allowed to create friend requests without confirming your email!");
            var target = await _dbContext.Users.FindAsync(id);
            if (target == null)
                return this.Protocol(ErrorType.NotFound, "We can not find your target user!");
            if (target.Id == user.Id)
                return this.Protocol(ErrorType.RequireAttention, "You can't request yourself!");
            var areFriends = await _dbContext.AreFriends(user.Id, target.Id);
            if (areFriends)
                return this.Protocol(ErrorType.HasDoneAlready, "You two are already friends!");
            Request request;
            lock (Obj)
            {
                var pending = _dbContext.Requests
                    .Where(t => t.CreatorId == user.Id)
                    .Where(t => t.TargetId == id)
                    .Any(t => !t.Completed);
                if (pending)
                    return this.Protocol(ErrorType.HasDoneAlready, "There are some pending request hasn't been completed!");
                request = new Request { CreatorId = user.Id, TargetId = id };
                _dbContext.Requests.Add(request);
                _dbContext.SaveChanges();
            }
            await _pusher.NewFriendRequestEvent(target.Id, user.Id);
            return this.AiurJson(new AiurValue<int>(request.Id)
            {
                Code = ErrorType.Success,
                Message = "Successfully created your request!"
            });
        }

        [HttpPost]
        public async Task<IActionResult> CompleteRequest(CompleteRequestAddressModel model)
        {
            var user = await GetKahlaUser();
            var request = await _dbContext.Requests.FindAsync(model.Id);
            if (request == null)
                return this.Protocol(ErrorType.NotFound, "We can not find target request.");
            if (request.TargetId != user.Id)
                return this.Protocol(ErrorType.Unauthorized, "The target user of this request is not you.");
            if (request.Completed)
                return this.Protocol(ErrorType.HasDoneAlready, "The target request is already completed.");
            request.Completed = true;
            if (model.Accept)
            {
                if (await _dbContext.AreFriends(request.CreatorId, request.TargetId))
                {
                    await _dbContext.SaveChangesAsync();
                    return this.Protocol(ErrorType.RequireAttention, "You two are already friends.");
                }
                _dbContext.AddFriend(request.CreatorId, request.TargetId);
                await _pusher.FriendAcceptedEvent(request.CreatorId);
            }
            await _dbContext.SaveChangesAsync();
            return this.Protocol(ErrorType.Success, "You have successfully completed this request.");
        }

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
            return this.AiurJson(new AiurCollection<Request>(requests)
            {
                Code = ErrorType.Success,
                Message = "Successfully get your requests list."
            });
        }

        public async Task<IActionResult> SearchEverything(SearchEverythingAddressModel model)
        {
            var user = await GetKahlaUser();
            var users = _dbContext
                .Users
                .AsNoTracking()
                .Where(t => t.NickName.ToLower().Contains(model.SearchInput.ToLower(), StringComparison.CurrentCultureIgnoreCase));

            var groups = _dbContext
                .GroupConversations
                .Include(t => t.Users)
                .AsNoTracking()
                .Where(t => t.GroupName.Contains(model.SearchInput.ToLower(), StringComparison.CurrentCultureIgnoreCase));

            var searched = SearchedGroup.Map(await groups.ToListAsync(), user.Id);

            return this.AiurJson(new SearchEverythingViewModel
            {
                UsersCount = await users.CountAsync(),
                GroupsCount = await groups.CountAsync(),
                Users = await users.Take(model.Take).ToListAsync(),
                Groups = searched,
                Code = ErrorType.Success,
                Message = "Search result is shown."
            });
        }

        public async Task<IActionResult> DiscoverFriends(int take = 15)
        {
            // Load everything to memory and even functions.
            var users = await _dbContext.Users
                .AsNoTracking()
                .ToListAsync();
            var conversations = await _dbContext.PrivateConversations
                .AsNoTracking()
                .ToListAsync();
            var requests = await _dbContext.Requests
                .AsNoTracking()
                .ToListAsync();
            bool AreFriends(string userId1, string userId2)
            {
                var relation = conversations.Any(t => t.RequesterId == userId1 && t.TargetId == userId2);
                if (relation) return true;
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
            bool SentRequest(string userId1, string userId2)
            {
                var relation = requests.Where(t => t.Completed == false).Any(t => t.CreatorId == userId1 && t.TargetId == userId2);
                if (relation) return true;
                var elation = requests.Where(t => t.Completed == false).Any(t => t.TargetId == userId1 && t.CreatorId == userId1);
                return elation;
            }

            var currentUser = await GetKahlaUser();
            var myFriends = HisPersonalFriendsId(currentUser.Id);
            var calculated = new List<FriendDiscovery>();
            foreach (var user in users)
            {
                if (user.Id == currentUser.Id || AreFriends(user.Id, currentUser.Id))
                {
                    continue;
                }
                var hisFriends = HisPersonalFriendsId(user.Id);
                var commonFriends = myFriends.Intersect(hisFriends).Count();
                if (commonFriends > 0)
                {
                    calculated.Add(new FriendDiscovery
                    {
                        CommonFriends = commonFriends,
                        TargetUser = user,
                        SentRequest = SentRequest(currentUser.Id, user.Id)
                    });
                }
                if (calculated.Count >= take)
                {
                    break;
                }
            }
            var ordered = calculated.OrderByDescending(t => t.CommonFriends);
            return this.AiurJson(new AiurCollection<FriendDiscovery>(ordered)
            {
                Code = ErrorType.Success,
                Message = "Successfully get your suggested friends."
            });
        }

        public async Task<IActionResult> UserDetail([Required]string id)
        {
            var user = await GetKahlaUser();
            var target = await _dbContext.Users.AsNoTracking().SingleOrDefaultAsync(t => t.Id == id);
            var model = new UserDetailViewModel();
            if (target == null)
            {
                model.Message = "We can not find target user.";
                model.Code = ErrorType.NotFound;
                return this.AiurJson(model);
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
            model.User = target;
            model.SentRequest = _dbContext.Requests.Any(t => t.TargetId == target.Id && t.CreatorId == user.Id);
            model.Message = "Found that user.";
            model.Code = ErrorType.Success;
            return this.AiurJson(model);
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

        private Task<KahlaUser> GetKahlaUser()
        {
            return _userManager.GetUserAsync(User);
        }
    }
}
