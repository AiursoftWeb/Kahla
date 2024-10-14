using Aiursoft.AiurProtocol.Models;
using Aiursoft.AiurProtocol.Server;
using Aiursoft.AiurProtocol.Server.Attributes;
using Aiursoft.DocGenerator.Attributes;
using Aiursoft.Kahla.SDK.Models.AddressModels;
using Aiursoft.Kahla.SDK.Models.Entities;
using Aiursoft.Kahla.SDK.Models.ViewModels;
using Aiursoft.Kahla.Server.Attributes;
using Aiursoft.Kahla.Server.Data;
using Aiursoft.Kahla.Server.Services.AppService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Controllers;

[KahlaForceAuth]
[GenerateDoc]
[ApiExceptionHandler(
    PassthroughRemoteErrors = true,
    PassthroughAiurServerException = true)]
[ApiModelStateChecker]
[Route("api/contacts")]
public class ContactsController(
    UserOthersViewAppService userAppService,
    ThreadJoinedViewAppService threadService,
    ILogger<ContactsController> logger,
    KahlaDbContext dbContext) : ControllerBase
{
    // This lock is used to avoid adding the same contact multiple times.
    private static readonly SemaphoreSlim AddFriendLock = new(1, 1);

    [HttpGet]
    [Route("mine")]
    [Produces<MyContactsViewModel>]
    public async Task<IActionResult> Mine([FromQuery]int skip = 0, [FromQuery]int take = 20)
    {
        var currentUserId = User.GetUserId();
        logger.LogInformation("User with Id: {Id} is trying to get all his known contacts.", currentUserId);
        var (totalCount, knownContacts) = await userAppService.GetMyContactsPagedAsync(currentUserId, skip, take);
        logger.LogInformation("User with Id: {Id} successfully get all his known contacts with total {Count}.", currentUserId, knownContacts.Count);
        return this.Protocol(new MyContactsViewModel
        {
            Code = Code.ResultShown,
            Message = $"Successfully get your first {take} known contacts and skipped {skip} contacts.",
            KnownContacts = knownContacts,
            TotalKnownContacts = totalCount
        });
    }

    [HttpGet]
    [Route("details/{id}")]
    public async Task<IActionResult> Details([FromRoute]string id, [FromQuery]int skip = 0, [FromQuery]int take = 20)
    {
        var currentUserId = User.GetUserId();
        logger.LogInformation("User with Id: {Id} is trying to download the detailed info with a contact with id: {TargetId}.", currentUserId, id);
        var searchedUser = await userAppService.GetUserByIdAsync(id, currentUserId);
        if (searchedUser == null)
        {
            logger.LogWarning("User with Id: {Id} is trying to download the detailed info with a contact with id: {TargetId} but the target does not exist.", currentUserId, id);
            return this.Protocol(Code.NotFound, $"The target user with id `{id}` does not exist.");
        }
        
        var (commonThreadsCount, commonThreads) = await threadService.QueryCommonThreadsAsync(
            viewingUserId: currentUserId,
            targetUserId: id,
            skip: skip,
            take: take);
        
        logger.LogInformation("User with Id: {Id} successfully downloaded the detailed info with a contact with id: {TargetId}.", currentUserId, id);
        return this.Protocol(new UserDetailViewModel 
        {
            SearchedUser = searchedUser,
            CommonThreadsCount = commonThreadsCount,
            CommonThreads = commonThreads,
            Code = Code.ResultShown,
            Message = $"User detail with first {take} common threads and skipped {skip} threads successfully downloaded."
        });
    }

    [HttpPost]
    [Route("add/{id}")]
    public async Task<IActionResult> AddContact([FromRoute] string id)
    {
        var currentUserId = User.GetUserId(); 
        logger.LogInformation("User with Id: {Id} is trying to add a new contact with id: {TargetId}.", currentUserId, id);
        var target = await dbContext.Users.FindAsync(id);
        if (target == null)
        {
            logger.LogWarning("User with Id: {Id} is trying to add a contact with id: {TargetId} but the target does not exist.", currentUserId, id);
            return this.Protocol(Code.NotFound, "The target user does not exist.");
        }
        
        logger.LogTrace("Waiting for the lock to add a new contact from id {SourceId} with id: {TargetId}.", currentUserId, target.Id);
        await AddFriendLock.WaitAsync();
        try
        {
            var duplicated =
                await dbContext.ContactRecords.AnyAsync(t => t.CreatorId == currentUserId && t.TargetId == target.Id);
            if (duplicated)
            {
                logger.LogWarning(
                    "User with Id: {Id} is trying to add a contact with id: {TargetId} but the target is already his contact.",
                    currentUserId, id);
                return this.Protocol(Code.Conflict, "The target user is already your known contact.");
            }

            var contactRecord = new ContactRecord
            {
                CreatorId = currentUserId,
                TargetId = target.Id,
                AddTime = DateTime.UtcNow
            };
            await dbContext.ContactRecords.AddAsync(contactRecord);
            await dbContext.SaveChangesAsync();
        }
        finally
        {
            AddFriendLock.Release();
            logger.LogTrace("Released the lock to add a new contact from id {SourceId} with id: {TargetId}.", currentUserId, id);
        }
        logger.LogInformation("User with Id: {Id} successfully added a new contact with id: {TargetId}.", currentUserId, id);
        return this.Protocol(Code.JobDone, "Successfully added the target user as your contact.");
    }
    
    [HttpPost]
    [Route("remove/{id}")]
    public async Task<IActionResult> RemoveContact(string id)
    {
        var currentUserId = User.GetUserId();
        logger.LogInformation("User with Id: {Id} is trying to remove a contact with id: {TargetId}.", currentUserId, id);
        var contactRecord = await dbContext.ContactRecords.SingleOrDefaultAsync(t => t.CreatorId == currentUserId && t.TargetId == id);
        if (contactRecord == null)
        {
            logger.LogWarning("User with Id: {Id} is trying to remove a contact with id: {TargetId} but the target is not his contact.", currentUserId, id);
            return this.Protocol(Code.NotFound, "The target user is not your known contact.");
        }
        dbContext.ContactRecords.Remove(contactRecord);
        await dbContext.SaveChangesAsync();
        logger.LogInformation("User with Id: {Id} successfully removed a contact with id: {TargetId}.", currentUserId, id);
        return this.Protocol(Code.JobDone, "Successfully removed the target user from your contacts.");
    }
    
    [HttpPost]
    [Route("report")]
    public async Task<IActionResult> ReportHim([FromForm]ReportHimAddressModel model)
    {
        var currentUserId = User.GetUserId();
        var targetUser = await dbContext.Users.SingleOrDefaultAsync(t => t.Id == model.TargetUserId);
        if (targetUser == null)
        {
            return this.Protocol(Code.NotFound, $"Could not find target user with id `{model.TargetUserId}`!");
        }
        if (currentUserId == targetUser.Id)
        {
            return this.Protocol(Code.Conflict, "You can not report yourself!");
        }
        var exists = await dbContext
            .Reports
            .AnyAsync((t) => t.TriggerId == currentUserId && t.TargetId == targetUser.Id && t.Status == ReportStatus.Pending);
        if (exists)
        {
            return this.Protocol(Code.NoActionTaken, "You have already reported the target user!");
        }
        // All check passed. Report him now!
        await dbContext.Reports.AddAsync(new Report
        {
            TargetId = targetUser.Id,
            TriggerId = currentUserId,
            Reason = model.Reason
        });
        await dbContext.SaveChangesAsync();
        return this.Protocol(Code.JobDone, "Successfully reported target user!");
    }
}

