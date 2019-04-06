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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.Server.Controllers
{
    [APIExpHandler]
    [APIModelStateChecker]
    [AiurForceAuth(directlyReject: true)]
    public class ConversationController : Controller
    {
        private readonly UserManager<KahlaUser> _userManager;
        private readonly KahlaDbContext _dbContext;
        private readonly KahlaPushService _pusher;

        public ConversationController(
            UserManager<KahlaUser> userManager,
            KahlaDbContext dbContext,
            KahlaPushService pushService)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _pusher = pushService;
        }

        public async Task<IActionResult> GetMessage([Required]int id, int skipTill = -1, int take = 15)
        {
            var user = await GetKahlaUser();
            var target = await _dbContext.Conversations.FindAsync(id);
            if (!await _dbContext.VerifyJoined(user.Id, target))
            {
                return this.Protocol(ErrorType.Unauthorized, "You don't have any relationship with that conversation.");
            }
            //Get Messages
            IQueryable<Message> allMessages = _dbContext
                .Messages
                .AsNoTracking()
                .Include(t => t.Conversation)
                .Where(t => t.ConversationId == target.Id)
                // Only messages within the life time.
                .Where(t => DateTime.UtcNow < t.SendTime + TimeSpan.FromSeconds(t.Conversation.MaxLiveSeconds));
            if (skipTill != -1)
            {
                allMessages = allMessages.Where(t => t.Id < skipTill);
            }

            var allMessagesList = await allMessages
                .OrderByDescending(t => t.Id)
                .Take(take)
                .OrderBy(t => t.Id)
                .ToListAsync();
            if (target.Discriminator == nameof(PrivateConversation))
            {
                await _dbContext.Messages
                    .Where(t => t.ConversationId == target.Id)
                    .Where(t => t.SenderId != user.Id)
                    .Where(t => t.Read == false)
                    .ForEachAsync(t => t.Read = true);
            }
            else if (target.Discriminator == nameof(GroupConversation))
            {
                var relation = await _dbContext.UserGroupRelations
                    .SingleOrDefaultAsync(t => t.UserId == user.Id && t.GroupId == target.Id);
                relation.ReadTimeStamp = DateTime.UtcNow;
            }
            await _dbContext.SaveChangesAsync();
            return this.AiurJson(new AiurCollection<Message>(allMessagesList)
            {
                Code = ErrorType.Success,
                Message = "Successfully get all your messages."
            });
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(SendMessageAddressModel model)
        {
            var user = await GetKahlaUser();
            var target = await _dbContext.Conversations.FindAsync(model.Id);
            if (!await _dbContext.VerifyJoined(user.Id, target))
            {
                return this.Protocol(ErrorType.Unauthorized, "You don't have any relationship with that conversation.");
            }

            if (model.Content.Trim().Length == 0)
            {
                return this.Protocol(ErrorType.InvalidInput, "Can not send empty message.");
            }
            //Create message.
            var message = new Message
            {
                Content = model.Content,
                SenderId = user.Id,
                ConversationId = target.Id
            };
            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync();
            switch (target)
            {
                //Push the message to receiver
                case PrivateConversation privateConversation:
                    {
                        var requester = await _userManager.FindByIdAsync(privateConversation.RequesterId);
                        var targetUser = await _userManager.FindByIdAsync(privateConversation.TargetId);
                        await _pusher.NewMessageEvent(requester, target, model.Content, user, true);
                        // In cause you are talking to yourself.
                        if (requester.Id != targetUser.Id)
                        {
                            await _pusher.NewMessageEvent(targetUser, target, model.Content, user, true);
                        }
                        break;
                    }
                case GroupConversation groupConversation:
                    {
                        var usersJoined = await _dbContext
                            .UserGroupRelations
                            .Include(t => t.User)
                            .Where(t => t.GroupId == groupConversation.Id)
                            .ToListAsync();
                        var taskList = new List<Task>();
                        foreach (var relation in usersJoined)
                        {
                            var pushTask = _pusher.NewMessageEvent(
                                receiver: relation.User,
                                conversation: target,
                                content: model.Content,
                                sender: user,
                                alert: !relation.Muted);
                            taskList.Add(pushTask);
                        }
                        await Task.WhenAll(taskList);
                        break;
                    }
            }
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return this.Protocol(ErrorType.RequireAttention, "Your message has been sent. But an error occured while sending web push notification.");
            }
            //Return success message.
            return this.Protocol(ErrorType.Success, "Your message has been sent.");
        }

        public async Task<IActionResult> ConversationDetail([Required]int id)
        {
            var user = await GetKahlaUser();
            var conversations = await _dbContext.MyConversations(user.Id);
            var target = conversations.SingleOrDefault(t => t.Id == id);
            if (target == null)
            {
                return this.Protocol(ErrorType.NotFound, "Could not find target conversation in your friends.");
            }
            target.DisplayName = target.GetDisplayName(user.Id);
            target.DisplayImage = target.GetDisplayImage(user.Id);
            if (target is PrivateConversation privateTarget)
            {
                privateTarget.AnotherUserId = privateTarget.AnotherUser(user.Id).Id;
                return this.AiurJson(new AiurValue<PrivateConversation>(privateTarget)
                {
                    Code = ErrorType.Success,
                    Message = "Successfully get target conversation."
                });
            }
            if (target is GroupConversation groupTarget)
            {
                var relations = await _dbContext
                    .UserGroupRelations
                    .AsNoTracking()
                    .Include(t => t.User)
                    .Where(t => t.GroupId == groupTarget.Id)
                    .ToListAsync();
                groupTarget.Users = relations;
                return this.AiurJson(new AiurValue<GroupConversation>(groupTarget)
                {
                    Code = ErrorType.Success,
                    Message = "Successfully get target conversation."
                });
            }
            throw new InvalidOperationException("Target is:" + target.Discriminator);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateMessageLifeTime(UpdateMessageLifeTimeAddressModel model)
        {
            var user = await GetKahlaUser();
            var target = await _dbContext.Conversations.FindAsync(model.Id);
            if (!await _dbContext.VerifyJoined(user.Id, target))
            {
                return this.Protocol(ErrorType.Unauthorized, "You don't have any relationship with that conversation.");
            }
            // Do update.
            target.MaxLiveSeconds = model.NewLifeTime;
            await _dbContext.SaveChangesAsync();
            if (target is PrivateConversation privateConversation)
            {
                var requester = await _userManager.FindByIdAsync(privateConversation.RequesterId);
                await _pusher.TimerUpdatedEvent(requester, model.NewLifeTime, target.Id);
                if (privateConversation.RequesterId != privateConversation.TargetId)
                {
                    var targetUser = await _userManager.FindByIdAsync(privateConversation.TargetId);
                    await _pusher.TimerUpdatedEvent(targetUser, model.NewLifeTime, target.Id);
                }
            }
            else if (target is GroupConversation g)
            {
                var usersJoined = await _dbContext
                    .UserGroupRelations
                    .Include(t => t.User)
                    .Where(t => t.GroupId == g.Id)
                    .ToListAsync();
                var taskList = new List<Task>();
                foreach (var relation in usersJoined)
                {
                    var pushTask = _pusher.TimerUpdatedEvent(relation.User, model.NewLifeTime, target.Id);
                    taskList.Add(pushTask);
                }
                await Task.WhenAll(taskList);
            }
            return this.Protocol(ErrorType.Success, "Successfully updated your life time. Your current message life time is: " +
                TimeSpan.FromSeconds(target.MaxLiveSeconds));
        }

        private Task<KahlaUser> GetKahlaUser()
        {
            return _userManager.GetUserAsync(User);
        }
    }
}
