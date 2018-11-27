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
        private readonly PushKahlaMessageService _pusher;

        public ConversationController(
            UserManager<KahlaUser> userManager,
            KahlaDbContext dbContext,
            PushKahlaMessageService pushService)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _pusher = pushService;
        }

        public async Task<IActionResult> GetMessage([Required]int id, int take = 15)
        {
            var user = await GetKahlaUser();
            var target = await _dbContext.Conversations.FindAsync(id);
            if (!await _dbContext.VerifyJoined(user.Id, target))
                return this.Protocal(ErrorType.Unauthorized, "You don't have any relationship with that conversation.");
            //Get Messages
            var allMessages = await _dbContext
                .Messages
                .AsNoTracking()
                .Where(t => t.ConversationId == target.Id)
                .Include(t => t.Sender)
                .OrderByDescending(t => t.SendTime)
                .Take(take)
                .OrderBy(t => t.SendTime)
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
            return this.AiurJson(new AiurCollection<Message>(allMessages)
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
                return this.Protocal(ErrorType.Unauthorized, "You don't have any relationship with that conversation.");
            if (model.Content.Trim().Length == 0)
                return this.Protocal(ErrorType.InvalidInput, "Can not send empty message.");
            //Create message.
            var message = new Message
            {
                Content = model.Content,
                SenderId = user.Id,
                ConversationId = target.Id
            };
            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync();
            //Push the message to reciever
            if (target.Discriminator == nameof(PrivateConversation))
            {
                var privateConversation = target as PrivateConversation;
                await _pusher.NewMessageEvent(privateConversation.RequesterId, target.Id, model.Content, user, target.AESKey);
                await _pusher.NewMessageEvent(privateConversation.TargetId, target.Id, model.Content, user, target.AESKey);
            }
            else if (target.Discriminator == nameof(GroupConversation))
            {
                var usersJoined = await _dbContext
                    .UserGroupRelations
                    .Include(t => t.User)
                    .Where(t => t.GroupId == target.Id)
                    .ToListAsync();
                var usersInGroup = usersJoined.Select(t => t.User).ToList();
                var taskList = new List<Task>();
                foreach (var relation in usersJoined)
                {
                    async Task sendNotification()
                    {
                        await _pusher.NewMessageEvent(relation.UserId, target.Id, model.Content, user, target.AESKey, relation.Muted, usersInGroup);
                    };
                    taskList.Add(sendNotification());
                }
                await Task.WhenAll(taskList);
            }
            //Return success message.
            return this.Protocal(ErrorType.Success, "Your message has been sent.");
        }

        public async Task<IActionResult> ConversationDetail([Required]int id)
        {
            var user = await GetKahlaUser();
            var conversations = await _dbContext.MyConversations(user.Id);
            var target = conversations.SingleOrDefault(t => t.Id == id);
            if (target == null)
            {
                return this.Protocal(ErrorType.NotFound, "Could not find target conversation in your friends.");
            }
            target.DisplayName = target.GetDisplayName(user.Id);
            target.DisplayImage = target.GetDisplayImage(user.Id);
            if (target is PrivateConversation)
            {
                var pTarget = target as PrivateConversation;
                pTarget.AnotherUserId = pTarget.AnotherUser(user.Id).Id;
                return this.AiurJson(new AiurValue<PrivateConversation>(pTarget)
                {
                    Code = ErrorType.Success,
                    Message = "Successfully get target conversation."
                });
            }
            else if (target is GroupConversation)
            {
                var gtarget = target as GroupConversation;
                var relations = await _dbContext
                    .UserGroupRelations
                    .AsNoTracking()
                    .Include(t => t.User)
                    .Where(t => t.GroupId == gtarget.Id)
                    .ToListAsync();
                gtarget.Users = relations;
                return this.AiurJson(new AiurValue<GroupConversation>(gtarget)
                {
                    Code = ErrorType.Success,
                    Message = "Successfully get target conversation."
                });
            }
            else
            {
                throw new NotImplementedException();
            }
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
