using Aiursoft.Archon.SDK.Services;
using Aiursoft.DocGenerator.Attributes;
using Aiursoft.Handler.Attributes;
using Aiursoft.Handler.Models;
using Aiursoft.Identity.Attributes;
using Aiursoft.Probe.SDK.Services;
using Aiursoft.Probe.SDK.Services.ToProbeServer;
using Aiursoft.SDKTools.Attributes;
using Aiursoft.WebTools;
using Kahla.SDK.Models;
using Kahla.SDK.Models.ApiAddressModels;
using Kahla.SDK.Models.ApiViewModels;
using Kahla.SDK.Services;
using Kahla.Server.Data;
using Kahla.Server.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
        private readonly FoldersService _foldersService;
        private readonly AppsContainer _appsContainer;
        private readonly IConfiguration _configuration;
        private readonly OnlineJudger _onlineJudger;
        private readonly LastSaidJudger _lastSaidJudger;
        private readonly ProbeLocator _probeLocator;

        public ConversationController(
            UserManager<KahlaUser> userManager,
            KahlaDbContext dbContext,
            KahlaPushService pushService,
            FoldersService foldersService,
            AppsContainer appsContainer,
            IConfiguration configuration,
            OnlineJudger onlineJudger,
            LastSaidJudger lastSaidJudger,
            ProbeLocator probeLocator)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _pusher = pushService;
            _foldersService = foldersService;
            _appsContainer = appsContainer;
            _configuration = configuration;
            _onlineJudger = onlineJudger;
            _lastSaidJudger = lastSaidJudger;
            _probeLocator = probeLocator;
        }

        [APIProduces(typeof(AiurCollection<ContactInfo>))]
        public async Task<IActionResult> All()
        {
            var user = await GetKahlaUser();
            var contacts = await _dbContext.MyContacts(user.Id).ToListAsync();
            foreach (var contact in contacts)
            {
                if (contact.LatestMessage != null)
                    contact.LatestMessage.Sender = contact.Sender.Build(_onlineJudger);
                contact.Online = contact.Discriminator == nameof(PrivateConversation) ?
                    _onlineJudger.IsOnline(contact.UserId, !contact.EnableInvisiable) : null;
            }
            return Json(new AiurCollection<ContactInfo>(contacts)
            {
                Code = ErrorType.Success,
                Message = "Successfully get all your friends."
            });
        }

        [APIProduces(typeof(AiurCollection<Message>))]
        public async Task<IActionResult> GetMessage([Required] int id, int take = 15, [IsGuidOrEmpty] string skipFrom = "")
        {
            var user = await GetKahlaUser();
            var target = await _dbContext
                .Conversations
                .Include(nameof(GroupConversation.Users))
                .SingleOrDefaultAsync(t => t.Id == id);
            if (target == null)
            {
                return this.Protocol(ErrorType.NotFound, $"Can not find conversation with id: {id}.");
            }
            if (!target.HasUser(user.Id))
            {
                return this.Protocol(ErrorType.Unauthorized, "You don't have any relationship with that conversation.");
            }
            var timeLimit = DateTime.UtcNow - TimeSpan.FromSeconds(target.MaxLiveSeconds);
            DateTime? skipStart = null;
            if (!string.IsNullOrWhiteSpace(skipFrom))
            {
                Guid.TryParse(skipFrom, out Guid guid);
                skipStart = (await _dbContext
                    .Messages
                    .AsNoTracking()
                    .Where(t => t.ConversationId == target.Id)
                    .SingleOrDefaultAsync(t => t.Id == guid))?.SendTime;
            }
            //Get Messages
            var allMessages = await _dbContext
                .Messages
                .AsNoTracking()
                .Include(t => t.Conversation)
                .Include(t => t.Ats)
                .Include(t => t.Sender)
                .Where(t => t.ConversationId == target.Id)
                .Where(t => t.SendTime > timeLimit)
                .Where(t => skipStart == null || t.SendTime < skipStart)
                .OrderByDescending(t => t.SendTime)
                .Take(take)
                .OrderBy(t => t.SendTime)
                .ToListAsync();
            var lastReadTime = await _dbContext.SetLastRead(target, user.Id);
            await _dbContext.SaveChangesAsync();
            allMessages.ForEach(t => t.Read = t.SendTime <= lastReadTime);
            allMessages.ForEach(t => t.Sender.Build(_onlineJudger));
            return Json(new AiurCollection<Message>(allMessages)
            {
                Code = ErrorType.Success,
                Message = "Successfully get all your messages."
            });
        }

        [HttpPost]
        [APIProduces(typeof(AiurValue<Message>))]
        public async Task<IActionResult> SendMessage(SendMessageAddressModel model)
        {
            // Ensure everything is safe.
            model.At ??= new string[0];
            var user = await GetKahlaUser();
            var target = await _dbContext
                .Conversations
                .Include(t => (t as PrivateConversation).RequestUser)
                .ThenInclude(t => t.HisDevices)
                .Include(t => (t as PrivateConversation).TargetUser)
                .ThenInclude(t => t.HisDevices)
                .Include(t => (t as GroupConversation).Users)
                .ThenInclude(t => t.User)
                .ThenInclude(t => t.HisDevices)
                .SingleOrDefaultAsync(t => t.Id == model.Id);
            if (target == null)
            {
                return this.Protocol(ErrorType.NotFound, $"Can not find conversation with id: {model.Id}.");
            }
            if (!target.HasUser(user.Id))
            {
                return this.Protocol(ErrorType.Unauthorized, "You don't have any relationship with that conversation.");
            }
            if (model.Content.Trim().Length == 0)
            {
                return this.Protocol(ErrorType.InvalidInput, "Can not send empty message.");
            }
            // Get last message Id.
            string lastMessageId = null;
            try
            {
                lastMessageId = _lastSaidJudger.LastMessageId(target.Id);
            }
            catch (ArgumentNullException)
            {
                Guid? nullableLastMessageId = await _dbContext
                    .Messages
                    .Where(t => t.ConversationId == target.Id)
                    .OrderByDescending(t => t.SendTime)
                    .Select(t => t.Id)
                    .FirstOrDefaultAsync();
                lastMessageId = nullableLastMessageId?.ToString() ?? null;
            }
            // Create message.
            var message = new Message
            {
                Id = Guid.Parse(model.MessageId),
                Content = model.Content,
                SenderId = user.Id,
                Sender = user.Build(_onlineJudger),
                ConversationId = target.Id,
                SendTime = DateTime.UtcNow,
                GroupWithPrevious = _lastSaidJudger.ShallBeGroupped(user.Id, target.Id)
            };
            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync();
            _lastSaidJudger.MarkSend(user.Id, target.Id, message.Id);
            // Create at info for this message.
            foreach (var atTargetId in model.At)
            {
                if (target.HasUser(atTargetId))
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
            // Save the ats.
            await _dbContext.SaveChangesAsync();
            // Set last read time.
            var lastReadTime = await _dbContext.SetLastRead(target, user.Id);
            await _dbContext.SaveChangesAsync();
            await target.ForEachUserAsync((eachUser, relation) =>
            {
                var mentioned = model.At.Contains(eachUser.Id);
                return _pusher.NewMessageEvent(
                                stargateChannel: eachUser.CurrentChannel,
                                devices: eachUser.HisDevices,
                                conversation: target,
                                message: message,
                                lastMessageId: lastMessageId,
                                pushAlert: eachUser.Id != user.Id && (mentioned || !(relation?.Muted ?? false)),
                                mentioned: mentioned
                                );
            });
            return Json(new AiurValue<Message>(message)
            {
                Code = ErrorType.Success,
                Message = "Your message has been sent."
            });
        }

        [APIProduces(typeof(AiurValue<PrivateConversation>))]
        [APIProduces(typeof(AiurValue<GroupConversation>))]
        public async Task<IActionResult> ConversationDetail([Required] int id)
        {
            var user = await GetKahlaUser();
            var target = await _dbContext
                .Conversations
                .Include(nameof(PrivateConversation.RequestUser))
                .Include(nameof(PrivateConversation.TargetUser))
                .Include(nameof(GroupConversation.Users))
                .Include(nameof(GroupConversation.Users) + "." + nameof(UserGroupRelation.User))
                .SingleOrDefaultAsync(t => t.Id == id);
            if (target == null)
            {
                return this.Protocol(ErrorType.NotFound, $"Can not find conversation with id: {id}.");
            }
            if (!target.HasUser(user.Id))
            {
                return this.Protocol(ErrorType.Unauthorized, "You don't have any relationship with that conversation.");
            }
            return Json(new AiurValue<Conversation>(target.Build(user.Id, _onlineJudger))
            {
                Code = ErrorType.Success,
                Message = "Successfully get target conversation."
            });
        }

        [APIProduces(typeof(FileHistoryViewModel))]
        public async Task<IActionResult> FileHistory([Required] int id, [Required] int skipDates)
        {
            var user = await GetKahlaUser();
            var conversation = await _dbContext
                .Conversations
                .Include(nameof(GroupConversation.Users))
                .SingleOrDefaultAsync(t => t.Id == id);
            if (conversation == null)
            {
                return this.Protocol(ErrorType.NotFound, $"Can not find conversation with id: {id}.");
            }
            if (!conversation.HasUser(user.Id))
            {
                return this.Protocol(ErrorType.Unauthorized, "You don't have any relationship with that conversation.");
            }
            var folders = await _foldersService
                .ViewContentAsync(await _appsContainer.AccessToken(), _configuration["UserFilesSiteName"], $"conversation-{conversation.Id}");
            var folder = folders.Value
                .SubFolders
                .OrderByDescending(t => DateTime.Parse(t.FolderName))
                .Skip(skipDates)
                .FirstOrDefault();
            if (folder == null)
            {
                return this.Protocol(ErrorType.RequireAttention, $"No files sent that day.");
            }
            var filesInSubfolder = await _foldersService.ViewContentAsync(await _appsContainer.AccessToken(), _configuration["UserFilesSiteName"], $"conversation-{conversation.Id}/{folder.FolderName}");
            return Json(new FileHistoryViewModel(filesInSubfolder.Value.Files.ToList())
            {
                Code = ErrorType.Success,
                ShowingDateUTC = folder.FolderName,
                Message = $"Successfully get all files that day in your conversation. Please download with pattern: '{_probeLocator.OpenFormat}'.",
                SiteName = _configuration["UserFilesSiteName"],
                RootPath = $"conversation-{conversation.Id}"
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateMessageLifeTime(UpdateMessageLifeTimeAddressModel model)
        {
            var user = await GetKahlaUser();
            var target = await _dbContext
                .Conversations
                .Include(t => (t as GroupConversation).Users)
                .ThenInclude(t => t.User)
                .SingleOrDefaultAsync(t => t.Id == model.Id);
            if (target == null)
            {
                return this.Protocol(ErrorType.NotFound, $"Can not find conversation with id: {model.Id}.");
            }
            if (!target.HasUser(user.Id))
            {
                return this.Protocol(ErrorType.Unauthorized, "You don't have any relationship with that conversation.");
            }
            if (target is GroupConversation g && g.OwnerId != user.Id)
            {
                return this.Protocol(ErrorType.Unauthorized, "You are not the owner of that group.");
            }
            var oldestAliveTime = DateTime.UtcNow - TimeSpan.FromSeconds(Math.Min(target.MaxLiveSeconds, model.NewLifeTime));
            // Delete outdated for current.
            var toDelete = await _dbContext
                .Messages
                .Where(t => t.ConversationId == target.Id)
                .Where(t => t.SendTime < oldestAliveTime)
                .ToListAsync();
            _dbContext.Messages.RemoveRange(toDelete);
            await _dbContext.SaveChangesAsync();
            // Update current.
            target.MaxLiveSeconds = model.NewLifeTime;
            await _dbContext.SaveChangesAsync();
            var taskList = new List<Task>();
            await target.ForEachUserAsync((eachUser, relation) =>
            {
                taskList.Add(_pusher.TimerUpdatedEvent(eachUser, model.NewLifeTime, target.Id));
                return Task.CompletedTask;
            });
            await Task.WhenAll(taskList);
            return this.Protocol(ErrorType.Success, "Successfully updated your life time. Your current message life time is: " +
                TimeSpan.FromSeconds(target.MaxLiveSeconds));
        }

        private Task<KahlaUser> GetKahlaUser() => _userManager.GetUserAsync(User);
    }
}
