using Aiursoft.Pylon;
using Aiursoft.Pylon.Attributes;
using Aiursoft.Pylon.Models;
using EFCore.BulkExtensions;
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
using System.Net;
using System.Threading.Tasks;

namespace Kahla.Server.Controllers
{
    [LimitPerMin(40)]
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

        [APIProduces(typeof(AiurCollection<ContactInfo>))]
        public async Task<IActionResult> All()
        {
            var user = await GetKahlaUser();
            var conversations = await _dbContext
                .MyConversations(user.Id);
            var list = conversations
                .Select(conversation => new ContactInfo
                {
                    ConversationId = conversation.Id,
                    DisplayName = conversation.GetDisplayName(user.Id),
                    DisplayImagePath = conversation.GetDisplayImagePath(user.Id),
                    LatestMessage = conversation.GetLatestMessage()?.Content ?? string.Empty,
                    LatestMessageTime = conversation.GetLatestMessage()?.SendTime ?? conversation.ConversationCreateTime,
                    UnReadAmount = conversation.GetUnReadAmount(user.Id),
                    Discriminator = conversation.Discriminator,
                    UserId = (conversation as PrivateConversation)?.AnotherUser(user.Id).Id,
                    AesKey = conversation.AESKey,
                    Muted = (conversation as GroupConversation)?.Users.FirstOrDefault(t => t.UserId == user.Id).Muted ?? false,
                    SomeoneAtMe = conversation.IWasAted(user.Id)
                })
                .OrderByDescending(t => t.SomeoneAtMe)
                .ThenByDescending(t => t.LatestMessageTime)
                .ToList();
            return Json(new AiurCollection<ContactInfo>(list)
            {
                Code = ErrorType.Success,
                Message = "Successfully get all your friends."
            });
        }

        [APIProduces(typeof(AiurCollection<Message>))]
        public async Task<IActionResult> GetMessage([Required]int id, int skipTill = -1, int take = 15)
        {
            var user = await GetKahlaUser();
            var target = await _dbContext
                .Conversations
                .SingleOrDefaultAsync(t => t.Id == id);
            if (!await target.Joined(_dbContext, user.Id))
            {
                return this.Protocol(ErrorType.Unauthorized, "You don't have any relationship with that conversation.");
            }
            //Get Messages
            var allMessages = await _dbContext
                .Messages
                .AsNoTracking()
                .Include(t => t.Conversation)
                .Include(t => t.Ats)
                .Include(t => t.Sender)
                .Where(t => t.ConversationId == target.Id)
                .Where(t => DateTime.UtcNow < t.SendTime + TimeSpan.FromSeconds(t.Conversation.MaxLiveSeconds))
                .Where(t => skipTill == -1 || t.Id < skipTill)
                .OrderByDescending(t => t.Id)
                .Take(take)
                .OrderBy(t => t.Id)
                .ToListAsync();
            var lastReadTime = await target.SetLastRead(_dbContext, user.Id);
            await _dbContext.SaveChangesAsync();
            allMessages.ForEach(t => t.Read = t.SendTime <= lastReadTime);
            return Json(new AiurCollection<Message>(allMessages)
            {
                Code = ErrorType.Success,
                Message = "Successfully get all your messages."
            });
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(SendMessageAddressModel model)
        {
            model.At = model.At ?? new string[0];
            var user = await GetKahlaUser();
            var target = await _dbContext.Conversations.FindAsync(model.Id);
            if (!await target.Joined(_dbContext, user.Id))
            {
                return this.Protocol(ErrorType.Unauthorized, "You don't have any relationship with that conversation.");
            }

            if (model.Content.Trim().Length == 0)
            {
                return this.Protocol(ErrorType.InvalidInput, "Can not send empty message.");
            }
            // Create message.
            var message = new Message
            {
                Content = model.Content,
                SenderId = user.Id,
                Sender = user,
                ConversationId = target.Id
            };
            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync();
            // Create at info for this message.
            foreach (var atTargetId in model.At)
            {
                if (await target.Joined(_dbContext, atTargetId))
                {
                    var at = new At
                    {
                        MessageId = message.Id,
                        TargetUserId = atTargetId
                    };
                    message.Ats.Add(at);
                    _dbContext.Ats.Add(at);
                }
                else
                {
                    _dbContext.Messages.Remove(message);
                    await _dbContext.SaveChangesAsync();
                    return this.Protocol(ErrorType.InvalidInput, $"Can not at person with Id: '{atTargetId}' because he is not in this conversation.");
                }
            }
            await _dbContext.SaveChangesAsync();

            try
            {
                await target.ForEachUserAsync((eachUser, relation) =>
                {
                    var mentioned = model.At.Contains(eachUser.Id);
                    return _pusher.NewMessageEvent(
                                    receiver: eachUser,
                                    conversation: target,
                                    message: message,
                                    muted: !mentioned && (relation?.Muted ?? false),
                                    mentioned: mentioned
                                    );
                }, _userManager);
            }
            catch (WebException e)
            {
                return this.Protocol(ErrorType.RequireAttention, "Your message has been sent. But an error occured while sending web push notification. " + e.Message);
            }
            //Return success message.
            return this.Protocol(ErrorType.Success, "Your message has been sent.");
        }

        [APIProduces(typeof(AiurValue<PrivateConversation>))]
        [APIProduces(typeof(AiurValue<GroupConversation>))]
        public async Task<IActionResult> ConversationDetail([Required]int id)
        {
            var user = await GetKahlaUser();
            var target = await _dbContext
                .Conversations
                .Include(nameof(PrivateConversation.RequestUser))
                .Include(nameof(PrivateConversation.TargetUser))
                .Include(nameof(GroupConversation.Users) + "." + nameof(UserGroupRelation.User))
                .SingleOrDefaultAsync(t => t.Id == id);
            if (!await target.Joined(_dbContext, user.Id))
            {
                return this.Protocol(ErrorType.Unauthorized, "You don't have any relationship with that conversation.");
            }
            return Json(new AiurValue<Conversation>(target.Build(user.Id))
            {
                Code = ErrorType.Success,
                Message = "Successfully get target conversation."
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateMessageLifeTime(UpdateMessageLifeTimeAddressModel model)
        {
            var user = await GetKahlaUser();
            var target = await _dbContext.Conversations.FindAsync(model.Id);
            if (!await target.Joined(_dbContext, user.Id))
            {
                return this.Protocol(ErrorType.Unauthorized, "You don't have any relationship with that conversation.");
            }
            if (target is GroupConversation g && g.OwnerId != user.Id)
            {
                return this.Protocol(ErrorType.Unauthorized, "You are not the owner of that group.");
            }
            var oldestAliveTime = DateTime.UtcNow - TimeSpan.FromSeconds(Math.Min(target.MaxLiveSeconds, model.NewLifeTime));
            // Delete outdated for current.
            var outdatedMessages = await _dbContext
                .Messages
                .Where(t => t.ConversationId == target.Id)
                .Where(t => t.SendTime < oldestAliveTime)
                .BatchDeleteAsync();
            await _dbContext.SaveChangesAsync();
            // Update current.
            target.MaxLiveSeconds = model.NewLifeTime;
            await _dbContext.SaveChangesAsync();
            var taskList = new List<Task>();
            await target.ForEachUserAsync((eachUser, relation) =>
            {
                taskList.Add(_pusher.TimerUpdatedEvent(eachUser, model.NewLifeTime, target.Id));
                return Task.CompletedTask;
            }, _userManager);
            await Task.WhenAll(taskList);
            return this.Protocol(ErrorType.Success, "Successfully updated your life time. Your current message life time is: " +
                TimeSpan.FromSeconds(target.MaxLiveSeconds));
        }

        private Task<KahlaUser> GetKahlaUser() => _userManager.GetUserAsync(User);
    }
}
