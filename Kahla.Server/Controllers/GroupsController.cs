using Aiursoft.Handler.Attributes;
using Aiursoft.Handler.Models;
using Aiursoft.Identity.Attributes;
using Aiursoft.Probe.SDK.Services.ToProbeServer;
using Aiursoft.WebTools;
using Kahla.SDK.Models;
using Kahla.SDK.Models.ApiAddressModels;
using Kahla.SDK.Models.ApiViewModels;
using Kahla.Server.Data;
using Kahla.Server.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Aiursoft.Canon;
using Aiursoft.Directory.SDK.Services;

namespace Kahla.Server.Controllers
{
    [LimitPerMin(40)]
    [APIRemoteExceptionHandler]
    [APIModelStateChecker]
    [AiurForceAuth(true)]
    public class GroupsController : ControllerBase
    {
        private readonly UserManager<KahlaUser> _userManager;
        private readonly KahlaDbContext _dbContext;
        private readonly OwnerChecker _ownerChecker;
        private readonly IConfiguration _configuration;
        private readonly FoldersService _foldersService;
        private readonly DirectoryAppTokenService _appsContainer;
        private readonly CanonQueue _cannonQueue;
        private static readonly object Obj = new();

        public GroupsController(
            UserManager<KahlaUser> userManager,
            KahlaDbContext dbContext,
            OwnerChecker ownerChecker,
            IConfiguration configuration,
            FoldersService foldersService,
            DirectoryAppTokenService appsContainer,
            CanonQueue cannonQueue)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _ownerChecker = ownerChecker;
            _configuration = configuration;
            _foldersService = foldersService;
            _appsContainer = appsContainer;
            _cannonQueue = cannonQueue;
        }

        [HttpPost]
        [Produces(typeof(AiurValue<int>))]
        public async Task<IActionResult> CreateGroupConversation(CreateGroupConversationAddressModel model)
        {
            var user = await GetKahlaUser();
            model.GroupName = model.GroupName.Trim();
            var exists = _dbContext.GroupConversations.Any(t => t.GroupName.ToLower() == model.GroupName.ToLower());
            if (exists)
            {
                return this.Protocol(ErrorType.Conflict, $"A group with name: {model.GroupName} was already exists!");
            }
            var limitedDate = DateTime.UtcNow - new TimeSpan(1, 0, 0, 0);
            var todayCreated = await _dbContext
                .GroupConversations
                .Where(t => t.OwnerId == user.Id)
                .Where(t => t.ConversationCreateTime > limitedDate)
                .CountAsync();
            if (todayCreated > 4)
            {
                return this.Protocol(ErrorType.TooManyRequests, "You have created too many groups today. Try it tomorrow!");
            }
            var createdGroup = await _dbContext.CreateGroup(model.GroupName, _configuration["GroupImagePath"], user.Id, model.JoinPassword);
            var newRelationship = new UserGroupRelation
            {
                UserId = user.Id,
                GroupId = createdGroup.Id,
                ReadTimeStamp = DateTime.MinValue
            };
            await _dbContext.UserGroupRelations.AddAsync(newRelationship);
            await _dbContext.SaveChangesAsync();
            _cannonQueue.QueueWithDependency<KahlaPushService>(pusher => 
                pusher.GroupJoinedEvent(user, createdGroup, null, 0));
            return this.Protocol(new AiurValue<int>(createdGroup.Id)
            {
                Code = ErrorType.Success,
                Message = "You have successfully created a new group and joined it!"
            });
        }

        [Produces(typeof(AiurValue<SearchedGroup>))]
        public async Task<IActionResult> GroupSummary([Required]int id)
        {
            var group = await _dbContext.GroupConversations.SingleOrDefaultAsync(t => t.Id == id);
            if (group == null)
            {
                return this.Protocol(ErrorType.NotFound, $"We can not find a group with id: {id}!");
            }
            var view = new SearchedGroup(group);
            return this.Protocol(new AiurValue<SearchedGroup>(view)
            {
                Code = ErrorType.Success,
                Message = "Successfully get your group result."
            });
        }

        [HttpPost]
        [Produces(typeof(AiurValue<int>))]
        public async Task<IActionResult> JoinGroup([Required]string groupName, string joinPassword)
        {
            var user = await GetKahlaUser();
            if (!user.EmailConfirmed)
            {
                return this.Protocol(ErrorType.Unauthorized, "You are not allowed to join groups without confirming your email!");
            }
            GroupConversation group;
            lock (Obj)
            {
                group = _dbContext
                   .GroupConversations
                   .Include(t => t.Users)
                   .ThenInclude(t => t.User)
                   .SingleOrDefault(t => t.GroupName == groupName);
                if (group == null)
                {
                    return this.Protocol(ErrorType.NotFound, $"We can not find a group with name: {groupName}!");
                }
                if (group.HasPassword && group.JoinPassword != joinPassword?.Trim())
                {
                    return this.Protocol(ErrorType.WrongKey, "The group requires password and your password was not correct!");
                }

                var joined = group.Users.Any(t => t.UserId == user.Id);
                if (joined)
                {
                    return this.Protocol(ErrorType.HasSuccessAlready, $"You have already joined the group: {groupName}!");
                }
                // All checked and able to join him.
                // Warning: Currently we do not have invitation system for invitation control is too complicated.
                var newRelationship = new UserGroupRelation
                {
                    UserId = user.Id,
                    GroupId = group.Id
                };
                _dbContext.UserGroupRelations.Add(newRelationship);
                _dbContext.SaveChanges();
            }
            await _dbContext.Entry(user)
                .Collection(t => t.HisDevices)
                .LoadAsync();
            var messagesCount = await _dbContext.Entry(group)
                .Collection(t => t.Messages)
                .Query()
                .CountAsync();
            var latestMessage = await _dbContext
                .Messages
                .Include(t => t.Sender)
                .OrderByDescending(t => t.SendTime)
                .FirstOrDefaultAsync();

            group.ForEachUser((eachUser, _) =>
                _cannonQueue.QueueWithDependency<KahlaPushService>(pusher =>
                    pusher.NewMemberEvent(eachUser, user, group.Id)));

            _cannonQueue.QueueWithDependency<KahlaPushService>(pusher =>
                pusher.GroupJoinedEvent(user, group, latestMessage, messagesCount));

            return this.Protocol(new AiurValue<int>(group.Id)
            {
                Code = ErrorType.Success,
                Message = $"You have successfully joint the group: {groupName}!"
            });
        }

        [HttpPost]
        public async Task<IActionResult> TransferGroupOwner([Required]string groupName, [Required]string targetUserId)
        {
            var user = await GetKahlaUser();
            var group = await _ownerChecker.FindMyOwnedGroupAsync(groupName, user.Id);
            // current user is the owner of the group.
            var targetRelationship = await _dbContext.GetRelationFromGroup(targetUserId, group.Id);
            if (targetRelationship == null)
            {
                return this.Protocol(ErrorType.NotFound, $"We can not find the target user with id: '{targetUserId}' in the group with name: '{groupName}'!");
            }
            if (group.OwnerId == targetUserId)
            {
                return this.Protocol(ErrorType.HasSuccessAlready, $"Caution! You are already the owner of the group '{groupName}'.");
            }
            group.OwnerId = targetUserId;
            await _dbContext.SaveChangesAsync();
            return this.Protocol(ErrorType.Success, $"Successfully transferred your group '{groupName}' ownership!");
        }

        [HttpPost]
        public async Task<IActionResult> KickMember([Required]string groupName, [Required]string targetUserId)
        {
            var user = await GetKahlaUser();
            var group = await _ownerChecker.FindMyOwnedGroupAsync(groupName, user.Id);
            var targetUser = await _dbContext
                .UserGroupRelations
                .SingleOrDefaultAsync(t => t.GroupId == group.Id && t.UserId == targetUserId);
            if (targetUser == null)
            {
                return this.Protocol(ErrorType.NotFound, $"We can not find the target user with id: '{targetUserId}' in the group with name: '{groupName}'!");
            }
            _dbContext.UserGroupRelations.Remove(targetUser);
            group.ForEachUser((eachUser, _) =>
                _cannonQueue.QueueWithDependency<KahlaPushService>(pusher =>
                    pusher.SomeoneLeftEvent(eachUser, targetUser.User, group.Id)));
            await _dbContext.SaveChangesAsync();
            return this.Protocol(ErrorType.Success, $"Successfully kicked the member from group '{groupName}'.");
        }

        [HttpPost]
        public async Task<IActionResult> DissolveGroup([Required]string groupName)
        {
            var user = await GetKahlaUser();
            var group = await _ownerChecker.FindMyOwnedGroupAsync(groupName, user.Id);

            group.ForEachUser((eachUser, _) =>
                _cannonQueue.QueueWithDependency<KahlaPushService>(pusher =>
                    pusher.DissolveEvent(eachUser, group.Id)));

            _dbContext.GroupConversations.Remove(group);
            await _dbContext.SaveChangesAsync();
            var token = await _appsContainer.GetAccessTokenAsync();
            var siteName = _configuration["UserFilesSiteName"];
            if ((await _foldersService.ViewContentAsync(token, siteName, "/")).Value.SubFolders.Any(f => f.FolderName == $"conversation-{group.Id}"))
            {
                await _foldersService.DeleteFolderAsync(token, siteName, $"conversation-{group.Id}");
            }
            return this.Protocol(ErrorType.Success, $"Successfully dissolved the group '{groupName}'!");
        }

        [HttpPost]
        public async Task<IActionResult> LeaveGroup([Required]string groupName)
        {
            var user = await GetKahlaUser();
            var group = await _dbContext.GroupConversations.SingleOrDefaultAsync(t => t.GroupName == groupName);
            if (group == null)
            {
                return this.Protocol(ErrorType.NotFound, $"We can not find a group with name: '{groupName}'!");
            }
            var joined = await _dbContext.GetRelationFromGroup(user.Id, group.Id);
            if (joined == null)
            {
                return this.Protocol(ErrorType.HasSuccessAlready, $"You did not joined the group: '{groupName}' at all!");
            }
            if (group.OwnerId == user.Id)
            {
                return this.Protocol(ErrorType.InsufficientPermissions, $"You are the owner of this group: '{groupName}' and you can't leave it!");
            }
            _dbContext.UserGroupRelations.Remove(joined);
            await _dbContext.SaveChangesAsync();

            group.ForEachUser((eachUser, _) =>
                _cannonQueue.QueueWithDependency<KahlaPushService>(pusher =>
                    pusher.SomeoneLeftEvent(eachUser, user, group.Id)));

            // Remove the group if no user in it.
            var any = _dbContext.UserGroupRelations.Any(t => t.GroupId == group.Id);
            if (!any)
            {
                _dbContext.GroupConversations.Remove(group);
                await _dbContext.SaveChangesAsync();
            }
            return this.Protocol(ErrorType.Success, $"You have successfully left the group: {groupName}!");
        }

        [HttpPost]
        public async Task<IActionResult> SetGroupMuted([Required]string groupName, [Required]bool setMuted)
        {
            var user = await GetKahlaUser();
            var group = await _dbContext.GroupConversations.SingleOrDefaultAsync(t => t.GroupName == groupName);
            if (group == null)
            {
                return this.Protocol(ErrorType.NotFound, $"We can not find a group with name: {groupName}!");
            }
            var joined = await _dbContext.GetRelationFromGroup(user.Id, group.Id);
            if (joined == null)
            {
                return this.Protocol(ErrorType.Unauthorized, $"You did not joined the group: {groupName} at all!");
            }
            if (joined.Muted == setMuted)
            {
                return this.Protocol(ErrorType.HasSuccessAlready, $"You have already {(joined.Muted ? "muted" : "unmuted")} the group: {groupName}!");
            }
            joined.Muted = setMuted;
            await _dbContext.SaveChangesAsync();
            return this.Protocol(ErrorType.Success, $"Successfully {(setMuted ? "muted" : "unmuted")} the group '{groupName}'!");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateGroupInfo(UpdateGroupAddressModel model)
        {
            var user = await GetKahlaUser();
            var group = await _ownerChecker.FindMyOwnedGroupAsync(model.GroupName, user.Id);
            if (!string.IsNullOrEmpty(model.NewName))
            {
                model.NewName = model.NewName.Trim();
                if (model.NewName != group.GroupName)
                {
                    if (_dbContext.GroupConversations.Any(t => t.GroupName.ToLower() == model.NewName.ToLower()))
                    {
                        return this.Protocol(ErrorType.Conflict, $"A group with name: '{model.NewName}' already exists!");
                    }
                    group.GroupName = model.NewName;
                }
            }
            if (!string.IsNullOrEmpty(model.AvatarPath))
            {
                group.GroupImagePath = model.AvatarPath;
            }
            group.ListInSearchResult = model.ListInSearchResult;
            await _dbContext.SaveChangesAsync();
            return this.Protocol(ErrorType.Success, $"Successfully updated the group '{model.GroupName}'.");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateGroupPassword(UpdateGroupPasswordAddressModel model)
        {
            var user = await GetKahlaUser();
            var group = await _ownerChecker.FindMyOwnedGroupAsync(model.GroupName, user.Id);
            group.JoinPassword = model.NewJoinPassword;
            await _dbContext.SaveChangesAsync();
            return this.Protocol(ErrorType.Success, $"Successfully update the join password of the group '{model.GroupName}'.");
        }

        private Task<KahlaUser> GetKahlaUser() => _userManager.GetUserAsync(User);
    }
}
