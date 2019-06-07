using Aiursoft.Pylon;
using Aiursoft.Pylon.Attributes;
using Aiursoft.Pylon.Models;
using Kahla.Server.Data;
using Kahla.Server.Models;
using Kahla.Server.Models.ApiAddressModels;
using Kahla.Server.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.Server.Controllers
{
    [APIExpHandler]
    [APIModelStateChecker]
    [AiurForceAuth(true)]
    public class GroupsController : Controller
    {
        private readonly UserManager<KahlaUser> _userManager;
        private readonly KahlaDbContext _dbContext;
        private readonly KahlaPushService _pusher;
        private static readonly object _obj = new object();

        public GroupsController(
            UserManager<KahlaUser> userManager,
            KahlaDbContext dbContext,
            KahlaPushService pusher)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _pusher = pusher;
        }

        [HttpPost]
        public async Task<IActionResult> CreateGroupConversation(CreateGroupConversationAddressModel model)
        {
            var user = await GetKahlaUser();
            if (!user.EmailConfirmed)
            {
                return this.Protocol(ErrorType.Unauthorized, "You are not allowed to join groups without confirming your email!");
            }
            model.GroupName = model.GroupName.Trim().ToLower();
            var exists = _dbContext.GroupConversations.Any(t => t.GroupName == model.GroupName);
            if (exists)
            {
                return this.Protocol(ErrorType.NotEnoughResources, $"A group with name: {model.GroupName} was already exists!");
            }
            var limitedDate = DateTime.UtcNow - new TimeSpan(1, 0, 0, 0);
            var todayCreated = await _dbContext
                .GroupConversations
                .Where(t => t.OwnerId == user.Id)
                .Where(t => t.ConversationCreateTime > limitedDate)
                .CountAsync();
            if (todayCreated > 4)
            {
                return this.Protocol(ErrorType.NotEnoughResources, "You have created too many groups today. Try it tomorrow!");
            }
            var createdGroup = await _dbContext.CreateGroup(model.GroupName, user.Id, model.JoinPassword);
            var newRelationship = new UserGroupRelation
            {
                UserId = user.Id,
                GroupId = createdGroup.Id,
                ReadTimeStamp = DateTime.MinValue
            };
            _dbContext.UserGroupRelations.Add(newRelationship);
            await _dbContext.SaveChangesAsync();
            return Json(new AiurValue<int>(createdGroup.Id)
            {
                Code = ErrorType.Success,
                Message = "You have successfully created a new group and joined it!"
            });
        }

        [HttpPost]
        public async Task<IActionResult> JoinGroup([Required]string groupName, string joinPassword)
        {
            var user = await GetKahlaUser();
            if (!user.EmailConfirmed)
            {
                return this.Protocol(ErrorType.Unauthorized, "You are not allowed to join groups without confirming your email!");
            }
            var group = await _dbContext
                .GroupConversations
                .Include(t => t.Users)
                .ThenInclude(t => t.User)
                .SingleOrDefaultAsync(t => t.GroupName == groupName);
            if (group == null)
            {
                return this.Protocol(ErrorType.NotFound, $"We can not find a group with name: {groupName}!");
            }
            if (group.HasPassword && group.JoinPassword != joinPassword?.Trim())
            {
                return this.Protocol(ErrorType.WrongKey, "The group requires password and your password was not correct!");
            }
            lock (_obj)
            {
                var joined = group.Users.Any(t => t.UserId == user.Id);
                if (joined)
                {
                    return this.Protocol(ErrorType.HasDoneAlready, $"You have already joined the group: {groupName}!");
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
            await group.ForEachUserAsync((eachUser, relation) => _pusher.NewMemberEvent(eachUser, user, group.Id), _userManager);
            return this.Protocol(ErrorType.Success, $"You have successfully joint the group: {groupName}!");
        }

        [HttpPost]
        public async Task<IActionResult> TransferGroupOwner([Required]string groupName, [Required]string targetUserId)
        {
            var user = await GetKahlaUser();
            var group = await _dbContext.GroupConversations.SingleOrDefaultAsync(t => t.GroupName == groupName);
            if (group == null)
            {
                return this.Protocol(ErrorType.NotFound, $"We can not find a group with name: '{groupName}'!");
            }
            if (group.OwnerId != user.Id)
            {
                return this.Protocol(ErrorType.Unauthorized, $"You are not the owner of this group: '{groupName}' and you can't transfer it!");
            }
            // current user is the owner of the group.
            var targetRelationship = await _dbContext.GetRelationFromGroup(targetUserId, group.Id);
            if (targetRelationship == null)
            {
                return this.Protocol(ErrorType.NotFound, $"We can not find the target user with id: '{targetUserId}' in the group with name: '{groupName}'!");
            }
            if (group.OwnerId == targetUserId)
            {
                return this.Protocol(ErrorType.RequireAttention, $"Caution! You are already the owner of the group '{groupName}'.");
            }
            group.OwnerId = targetUserId;
            await _dbContext.SaveChangesAsync();
            return this.Protocol(ErrorType.Success, $"Successfully transfered your group '{groupName}' ownership!");
        }

        [HttpPost]
        public async Task<IActionResult> KickMember([Required]string groupName, [Required]string targetUserId)
        {
            var user = await GetKahlaUser();

            var group = await _dbContext.GroupConversations.SingleOrDefaultAsync(t => t.GroupName == groupName);
            if (group == null)
            {
                return this.Protocol(ErrorType.NotFound, $"We can not find a group with name: '{groupName}'!");
            }
            if (group.OwnerId != user.Id)
            {
                return this.Protocol(ErrorType.Unauthorized, $"You are not the owner of this group: '{groupName}' and you can't kick out members.!");
            }
            var targetuser = await _dbContext
                .UserGroupRelations
                .SingleOrDefaultAsync(t => t.GroupId == group.Id && t.UserId == targetUserId);
            if (targetuser == null)
            {
                return this.Protocol(ErrorType.NotFound, $"We can not find the target user with id: '{targetUserId}' in the group with name: '{groupName}'!");
            }
            _dbContext.UserGroupRelations.Remove(targetuser);
            await group.ForEachUserAsync((eachUser, relation) => _pusher.SomeoneLeftEvent(eachUser, targetuser.User, group.Id), _userManager);
            await _dbContext.SaveChangesAsync();
            return this.Protocol(ErrorType.Success, $"Successfully kicked the member from group '{groupName}'.");
        }

        [HttpPost]
        public async Task<IActionResult> DissolveGroup([Required]string groupName)
        {
            var user = await GetKahlaUser();

            var group = await _dbContext.GroupConversations.SingleOrDefaultAsync(t => t.GroupName == groupName);
            if (group == null)
            {
                return this.Protocol(ErrorType.NotFound, $"We can not find a group with name: '{groupName}'!");
            }
            if (group.OwnerId != user.Id)
            {
                return this.Protocol(ErrorType.Unauthorized, $"You are not the owner of this group: '{groupName}' and you can't dissolve group.!");
            }

            _dbContext.GroupConversations.Remove(group);
            await group.ForEachUserAsync((eachUser, relation) => _pusher.DissolveEvent(eachUser, group.Id), _userManager);
            await _dbContext.SaveChangesAsync();
            return this.Protocol(ErrorType.Success, $"Successfully dissolve the group '{groupName}'.");
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
                return this.Protocol(ErrorType.HasDoneAlready, $"You did not joined the group: '{groupName}' at all!");
            }
            if (group.OwnerId == user.Id)
            {
                return this.Protocol(ErrorType.NotEnoughResources, $"You are the owner of this group: '{groupName}' and you can't leave it!");
            }
            _dbContext.UserGroupRelations.Remove(joined);
            await _dbContext.SaveChangesAsync();
            // Remove the group if no users in it.
            await group.ForEachUserAsync((eachUser, relation) => _pusher.SomeoneLeftEvent(eachUser, user, group.Id), _userManager);
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
                return this.Protocol(ErrorType.HasDoneAlready, $"You have already {(joined.Muted ? "muted" : "unmuted")} the group: {groupName}!");
            }
            joined.Muted = setMuted;
            await _dbContext.SaveChangesAsync();
            return this.Protocol(ErrorType.Success, $"Successfully {(setMuted ? "muted" : "unmuted")} the group '{groupName}'!");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateGroupInfo(UpdateGroupAddressModel model)
        {
            var user = await GetKahlaUser();
            var group = await _dbContext.GroupConversations.SingleOrDefaultAsync(t => t.GroupName == model.GroupName);
            if (group == null)
            {
                return this.Protocol(ErrorType.NotFound, $"Can not find a group with name: {model.GroupName}!");
            }
            if (group.OwnerId != user.Id)
            {
                return this.Protocol(ErrorType.Unauthorized, "You can't change settings for this group for you are not the owner of it.");
            }

            if (!string.IsNullOrEmpty(model.NewName))
            {
                model.NewName = model.NewName.Trim().ToLower();
                if (model.NewName != group.GroupName)
                {
                    if (_dbContext.GroupConversations.Any(t => t.GroupName == model.NewName))
                    {
                        return this.Protocol(ErrorType.NotEnoughResources, $"A group with name: '{model.NewName}' already exists!");
                    }
                    group.GroupName = model.NewName;
                }
            }

            if (model.AvatarKey != null)
            {
                group.GroupImageKey = model.AvatarKey.Value;
            }

            await _dbContext.SaveChangesAsync();
            return this.Protocol(ErrorType.Success, $"Successfully updated the name of the group '{model.GroupName}'.");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateGroupPassword(UpdateGroupPasswordAddressModel model)
        {
            var user = await GetKahlaUser();
            var group = await _dbContext.GroupConversations.SingleOrDefaultAsync(t => t.GroupName == model.GroupName);
            if (group == null)
            {
                return this.Protocol(ErrorType.NotFound, $"Can not find a group with name: '{model.GroupName}'!");
            }
            if (group.OwnerId != user.Id)
            {
                return this.Protocol(ErrorType.Unauthorized, "You can't change settings for this group for you are not the owner of it.");
            }
            group.JoinPassword = model.NewJoinPassword;
            await _dbContext.SaveChangesAsync();
            return this.Protocol(ErrorType.Success, $"Successfully update the join password of the group '{model.GroupName}'.");
        }

        private Task<KahlaUser> GetKahlaUser() => _userManager.GetUserAsync(User);
    }
}
