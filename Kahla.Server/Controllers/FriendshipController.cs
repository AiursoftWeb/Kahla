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
        private readonly PushKahlaMessageService _pusher;
        private static object _obj = new object();

        public FriendshipController(
            UserManager<KahlaUser> userManager,
            KahlaDbContext dbContext,
            PushKahlaMessageService pushService)
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
                    UserId = conversation is PrivateConversation ? (conversation as PrivateConversation).AnotherUser(user.Id).Id : null,
                    AesKey = conversation.AESKey,
                    Muted = conversation is GroupConversation ? (await _dbContext.GetRelationFromGroup(user.Id, conversation.Id)).Muted : false
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
                return this.Protocal(ErrorType.NotFound, "We can not find target user.");
            if (!await _dbContext.AreFriends(user.Id, target.Id))
                return this.Protocal(ErrorType.NotEnoughResources, "He is not your friend at all.");
            await _dbContext.RemoveFriend(user.Id, target.Id);
            await _dbContext.SaveChangesAsync();
            await _pusher.WereDeletedEvent(target.Id);
            return this.Protocal(ErrorType.Success, "Successfully deleted your friend relationship.");
        }

        [HttpPost]
        public async Task<IActionResult> CreateRequest([Required]string id)
        {
            var user = await GetKahlaUser();
            var target = await _dbContext.Users.FindAsync(id);
            if (target == null)
                return this.Protocal(ErrorType.NotFound, "We can not find your target user!");
            if (target.Id == user.Id)
                return this.Protocal(ErrorType.RequireAttention, "You can't request yourself!");
            var areFriends = await _dbContext.AreFriends(user.Id, target.Id);
            if (areFriends)
                return this.Protocal(ErrorType.HasDoneAlready, "You two are already friends!");
            Request request = null;
            lock (_obj)
            {
                var pending = _dbContext.Requests
                    .Where(t => t.CreatorId == user.Id)
                    .Where(t => t.TargetId == id)
                    .Any(t => !t.Completed);
                if (pending)
                    return this.Protocal(ErrorType.HasDoneAlready, "There are some pending request hasn't been completed!");
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
                return this.Protocal(ErrorType.NotFound, "We can not find target request.");
            if (request.TargetId != user.Id)
                return this.Protocal(ErrorType.Unauthorized, "The target user of this request is not you.");
            if (request.Completed == true)
                return this.Protocal(ErrorType.HasDoneAlready, "The target request is already completed.");
            request.Completed = true;
            if (model.Accept)
            {
                if (await _dbContext.AreFriends(request.CreatorId, request.TargetId))
                {
                    await _dbContext.SaveChangesAsync();
                    return this.Protocal(ErrorType.RequireAttention, "You two are already friends.");
                }
                _dbContext.AddFriend(request.CreatorId, request.TargetId);
                await _pusher.FriendAcceptedEvent(request.CreatorId);
            }
            await _dbContext.SaveChangesAsync();
            return this.Protocal(ErrorType.Success, "You have successfully completed this request.");
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

        public async Task<IActionResult> SearchFriends(SearchFriendsAddressModel model)
        {
            var users = await _dbContext
                .Users
                .AsNoTracking()
                .Where(t => t.NickName.Contains(model.NickName, StringComparison.CurrentCultureIgnoreCase))
                .Take(model.Take)
                .ToListAsync();

            return this.AiurJson(new AiurCollection<KahlaUser>(users)
            {
                Code = ErrorType.Success,
                Message = "Search result is shown."
            });
        }

        public async Task<IActionResult> DiscoverFriends()
        {
            var cuser = await GetKahlaUser();
            var myfriends = await _dbContext.MyPersonalFriendsId(cuser.Id);
            var calculated = new List<KeyValuePair<int, KahlaUser>>();
            foreach (var user in await _dbContext.Users.ToListAsync())
            {
                if (await _dbContext.AreFriends(user.Id, cuser.Id))
                {
                    continue;
                }
                var hisfriends = await _dbContext.MyPersonalFriendsId(user.Id);
                calculated.Add(new KeyValuePair<int, KahlaUser>(myfriends.Intersect(hisfriends).Count(), user));
            }
            var ordered = calculated.OrderByDescending(t => t.Key);
            return this.AiurJson(new AiurCollection<KeyValuePair<int, KahlaUser>>(ordered)
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
            model.Message = "Found that user.";
            model.Code = ErrorType.Success;
            return this.AiurJson(model);
        }

        [HttpPost]
        public async Task<IActionResult> ReportHim(ReportHimAddressModel model)
        {
            var cuser = await GetKahlaUser();
            var targetUser = await _dbContext.Users.SingleOrDefaultAsync(t => t.Id == model.TargetUserId);
            if (targetUser == null)
            {
                return this.Protocal(ErrorType.NotFound, $"Could not find target user with id `{model.TargetUserId}`!");
            }
            if (cuser.Id == targetUser.Id)
            {
                return this.Protocal(ErrorType.HasDoneAlready, $"You can not report yourself!");
            }
            var exists = await _dbContext
                .Reports
                .AnyAsync((t) => t.TriggerId == cuser.Id && t.TargetId == targetUser.Id && t.Status == ReportStatus.Pending);
            if (exists)
            {
                return this.Protocal(ErrorType.HasDoneAlready, "You have already reported the target user!");
            }
            // All chedk passed. Report him now!
            _dbContext.Reports.Add(new Report
            {
                TargetId = targetUser.Id,
                TriggerId = cuser.Id,
                Reason = model.Reason
            });
            await _dbContext.SaveChangesAsync();
            return this.Protocal(ErrorType.Success, "Successfully reported target user!");
        }

        private async Task<KahlaUser> GetKahlaUser()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return null;
            }
            return await _userManager.FindByNameAsync(User.Identity.Name);
        }
    }
}
