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

        public GroupsController(
            UserManager<KahlaUser> userManager,
            KahlaDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
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
            var group = await _dbContext.GroupConversations.SingleOrDefaultAsync(t => t.GroupName == groupName);
            if (group == null)
            {
                return this.Protocol(ErrorType.NotFound, $"We can not find a group with name: {groupName}!");
            }
            var joined = _dbContext.UserGroupRelations.Any(t => t.UserId == user.Id && t.GroupId == group.Id);
            if (joined)
            {
                return this.Protocol(ErrorType.HasDoneAlready, $"You have already joined the group: {groupName}!");
            }
            if (group.HasPassword && group.JoinPassword != joinPassword?.Trim())
            {
                return this.Protocol(ErrorType.WrongKey, "The group requires password and your password was not correct!");
            }
            // All checked and able to join him.
            // Warning: Currently we do not have invitation system for invitation control is too complicated.
            var newRelationship = new UserGroupRelation
            {
                UserId = user.Id,
                GroupId = group.Id
            };
            _dbContext.UserGroupRelations.Add(newRelationship);
            await _dbContext.SaveChangesAsync();
            return this.Protocol(ErrorType.Success, $"You have successfully joint the group: {groupName}!");
        }

        [HttpPost]
        public async Task<IActionResult> LeaveGroup([Required]string groupName)
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
                return this.Protocol(ErrorType.HasDoneAlready, $"You did not joined the group: {groupName} at all!");
            }
            _dbContext.UserGroupRelations.Remove(joined);
            await _dbContext.SaveChangesAsync();
            // Remove the group if no users in it.
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
                return this.Protocol(ErrorType.NotFound, $"We can not find a group with name: {model.GroupName}!");
            }

            if (group.OwnerId != user.Id)
            {
                return this.Protocol(ErrorType.RequireAttention, "You haven't the permission to execute this command.");
            }

            var shouldSave = false;

            if (model.AvatarKey != null)
            {
                group.GroupImageKey = model.AvatarKey.Value;
                shouldSave = true;
            }

            if (model.NewJoinPassword != null) {
                group.JoinPassword = model.NewJoinPassword;
                shouldSave = true;
            }

            if (shouldSave)
            {
                await _dbContext.SaveChangesAsync();
            }

            return this.Protocol(ErrorType.Success, $"Successfully update the info of the group '{model.GroupName}'.");
        }

        private Task<KahlaUser> GetKahlaUser() => _userManager.GetUserAsync(User);
    }
}
