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

        public async Task<IActionResult> All()
        {
            var user = await GetKahlaUser();
            var conversations = await _dbContext
                .MyConversations(user.Id);
            var list = conversations
                .Where(t => t.GetLatestMessage() != null)
                .Select(conversation => new ContactInfo
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
                    Muted = (conversation as GroupConversation)?.Users.FirstOrDefault(t => t.UserId == user.Id).Muted ?? false,
                    SomeoneAtMe = conversation.IWasAted(user.Id)
                })
                .OrderByDescending(t => t.SomeoneAtMe)
                .ThenByDescending(t => t.LatestMessageTime);
            return Json(new AiurCollection<ContactInfo>(list)
            {
                Code = ErrorType.Success,
                Message = "Successfully get all your friends."
            });
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
                .Include(t => t.Ats)
                .Include(t => t.Sender)
                .Where(t => t.ConversationId == target.Id)
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
                allMessagesList.ForEach(t => t.Read = t.SendTime < relation.ReadTimeStamp);
                relation.ReadTimeStamp = DateTime.UtcNow;
            }
            await _dbContext.SaveChangesAsync();
            return Json(new AiurCollection<Message>(allMessagesList)
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
            if (!await _dbContext.VerifyJoined(user.Id, target))
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
                ConversationId = target.Id
            };
            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync();
            // Create at info for this message.
            foreach (var atTargetId in model.At)
            {
                if (await _dbContext.VerifyJoined(atTargetId, target))
                {
                    var at = new At
                    {
                        MessageId = message.Id,
                        TargetUserId = atTargetId
                    };
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
            await target.ForEachUserAsync(async (eachUser, relation) =>
            {
                var mentioned = model.At.Contains(eachUser.Id);
                await _pusher.NewMessageEvent(
                                receiver: eachUser,
                                conversation: target,
                                content: model.Content,
                                sender: user,
                                muted: !mentioned && (relation?.Muted ?? false),
                                mentioned: mentioned
                                );
            }, _userManager);
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
                return Json(new AiurValue<PrivateConversation>(privateTarget)
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
                    .OrderByDescending(t => t.UserId == t.Group.OwnerId)
                    .ThenBy(t => t.JoinTime)
                    .ToListAsync();
                groupTarget.Users = relations;
                return Json(new AiurValue<GroupConversation>(groupTarget)
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
            if (target is GroupConversation g && g.OwnerId != user.Id)
            {
                return this.Protocol(ErrorType.Unauthorized, "You are not the owner of that group.");
            }
            // Delete outdated for current.
            await _dbContext
                .Messages
                .Include(t => t.Conversation)
                .Where(t => t.ConversationId == target.Id &&
                    (
                        DateTime.UtcNow > t.SendTime + TimeSpan.FromSeconds(t.Conversation.MaxLiveSeconds) 
                        ||
                        DateTime.UtcNow > t.SendTime + TimeSpan.FromSeconds(model.NewLifeTime)
                    )
                )
                .BatchDeleteAsync();
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
