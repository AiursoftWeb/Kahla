using Aiursoft.AiurProtocol.Models;
using Aiursoft.AiurProtocol.Server;
using Aiursoft.AiurProtocol.Server.Attributes;
using Aiursoft.DocGenerator.Attributes;
using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Models.AddressModels;
using Aiursoft.Kahla.SDK.Models.ViewModels;
using Aiursoft.Kahla.SDK.ModelsOBS;
using Aiursoft.Kahla.Server.Attributes;
using Aiursoft.Kahla.Server.Data;
using Aiursoft.Kahla.Server.Services.AppService;
using Aiursoft.Kahla.Server.Services.Mappers;
using Microsoft.AspNetCore.Identity;
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
    UserOthersViewAppService usersAppAppService,
    ThreadOthersViewAppService threadsAppService,
    UserDetailedViewAppService userDetailedViewAppService,
    ILogger<ContactsController> logger,
    KahlaMapper kahlaMapper,
    KahlaDbContext dbContext,
    UserManager<KahlaUser> userManager) : ControllerBase
{
    // This lock is used to prevent adding the same friend twice.
    private static readonly SemaphoreSlim AddFriendLock = new(1, 1);

    [HttpGet]
    [Route("mine")]
    [Produces<MyContactsViewModel>]
    public async Task<IActionResult> Mine([FromQuery]int take = 20)
    {
        var user = await this.GetCurrentUser(userManager);
        logger.LogInformation("User with email: {Email} is trying to get all his known contacts.", user.Email);
        var knownContacts = await usersAppAppService.GetMyContactsPagedAsync(user.Id, take);
        logger.LogInformation("User with email: {Email} successfully get all his known contacts with total {Count}.", user.Email, knownContacts.Count);
        return this.Protocol(new MyContactsViewModel
        {
            Code = Code.ResultShown,
            Message = "Successfully get all your known contacts.",
            KnownContacts = knownContacts
        });
    }
    
    [HttpPost]
    [Route("search")]
    [Produces(typeof(SearchEverythingViewModel))]
    public async Task<IActionResult> SearchEverything(SearchEverythingAddressModel model)
    {
        var user = await this.GetCurrentUser(userManager);
        logger.LogInformation("User with email: {Email} is trying to search for {SearchInput}. Take: {Take}.", user.Email, model.SearchInput, model.Take);
        
        var (totalUsersCount, users) = await usersAppAppService.SearchUsersPagedAsync(model.SearchInput, user.Id, model.Take);
        logger.LogInformation("User with email: {Email} successfully searched {Count} users.", user.Email, users.Count);
        
        // // TODO: Use app service.
        // var threadsQuery = dbContext
        //     .ChatThreads
        //     .AsNoTracking()
        //     .Where(t => t.AllowSearchByName || t.Id.ToString() == model.SearchInput)
        //     .Where(t => 
        //         t.Name.Contains(model.SearchInput) ||
        //         t.Id.ToString() == model.SearchInput);
        // var threadsEntities = await threadsQuery
        //     .Take(model.Take)
        //     .ToListAsync();
        // var threadsView = await threadsEntities
        //     .SelectAsListAsync(kahlaMapper.MapSearchedThreadAsync);
        var (totalThreadsCount, threads) = await threadsAppService.SearchThreadsPagedAsync(model.SearchInput, user.Id, model.Take);
        logger.LogInformation("User with email: {Email} successfully get {Count} threads.", user.Email, threads.Count);
    
        return this.Protocol(new SearchEverythingViewModel
        {
            TotalUsersCount = totalUsersCount,
            TotalThreadsCount = totalThreadsCount,
            Users = users,
            Threads = threads,
            Code = Code.ResultShown,
            Message = "Search result is shown."
        });
    }

    [HttpGet]
    [Route("details/{id}")]
    public async Task<IActionResult> Details([FromRoute]string id, [FromQuery]int takeThreads = 5)
    {
        var user = await this.GetCurrentUser(userManager);
        logger.LogInformation("User with email: {Email} is trying to download the detailed info with a contact with id: {TargetId}.", user.Email, id);
        var mapped = await userDetailedViewAppService.GetUserDetailedViewAsync(id, user.Id, takeThreads);
        if (mapped == null)
        {
            logger.LogWarning("User with email: {Email} is trying to download the detailed info with a contact with id: {TargetId} but the target does not exist.", user.Email, id);
            return this.Protocol(Code.NotFound, "The target user with id `{id}` does not exist.");
        }
        logger.LogInformation("User with email: {Email} successfully downloaded the detailed info with a contact with id: {TargetId}.", user.Email, id);
        return this.Protocol(new UserDetailViewModel 
        {
            DetailedUser = mapped,
            Code = Code.ResultShown,
            Message = $"User detail with first {takeThreads} common threads are shown."
        });
    }

    [HttpPost]
    [Route("add/{id}")]
    public async Task<IActionResult> AddContact([FromRoute] string id)
    {
        var user = await this.GetCurrentUser(userManager);
        logger.LogInformation("User with email: {Email} is trying to add a new contact with id: {TargetId}.", user.Email, id);
        var target = await dbContext.Users.FindAsync(id);
        if (target == null)
        {
            logger.LogWarning("User with email: {Email} is trying to add a contact with id: {TargetId} but the target does not exist.", user.Email, id);
            return this.Protocol(Code.NotFound, "The target user does not exist.");
        }
        
        logger.LogTrace("Waiting for the lock to add a new contact from id {SourceId} with id: {TargetId}.", user.Id, id);
        await AddFriendLock.WaitAsync();
        try
        {
            var duplicated =
                await dbContext.ContactRecords.AnyAsync(t => t.CreatorId == user.Id && t.TargetId == target.Id);
            if (duplicated)
            {
                logger.LogWarning(
                    "User with email: {Email} is trying to add a contact with id: {TargetId} but the target is already his contact.",
                    user.Email, id);
                return this.Protocol(Code.Conflict, "The target user is already your known contact.");
            }

            var contactRecord = new ContactRecord
            {
                CreatorId = user.Id,
                TargetId = target.Id,
                AddTime = DateTime.UtcNow
            };
            await dbContext.ContactRecords.AddAsync(contactRecord);
            await dbContext.SaveChangesAsync();
        }
        finally
        {
            AddFriendLock.Release();
            logger.LogTrace("Released the lock to add a new contact from id {SourceId} with id: {TargetId}.", user.Id, id);
        }
        logger.LogInformation("User with email: {Email} successfully added a new contact with id: {TargetId}.", user.Email, id);
        return this.Protocol(Code.JobDone, "Successfully added the target user as your contact. Please call the 'mine' API to get the latest information.");
    }
    
    [HttpPost]
    [Route("remove/{id}")]
    public async Task<IActionResult> RemoveContact(string id)
    {
        var user = await this.GetCurrentUser(userManager);
        logger.LogInformation("User with email: {Email} is trying to remove a contact with id: {TargetId}.", user.Email, id);
        var contactRecord = await dbContext.ContactRecords.SingleOrDefaultAsync(t => t.CreatorId == user.Id && t.TargetId == id);
        if (contactRecord == null)
        {
            logger.LogWarning("User with email: {Email} is trying to remove a contact with id: {TargetId} but the target is not his contact.", user.Email, id);
            return this.Protocol(Code.NotFound, "The target user is not your known contact.");
        }
        dbContext.ContactRecords.Remove(contactRecord);
        await dbContext.SaveChangesAsync();
        logger.LogInformation("User with email: {Email} successfully removed a contact with id: {TargetId}.", user.Email, id);
        return this.Protocol(Code.JobDone, "Successfully removed the target user from your contacts. Please call the 'mine' API to get the latest information.");
    }
    
    
    [HttpPost]
    [Route("report")]
    public async Task<IActionResult> ReportHim(ReportHimAddressModel model)
    {
        var currentUser = await this.GetCurrentUser(userManager);
        var targetUser = await dbContext.Users.SingleOrDefaultAsync(t => t.Id == model.TargetUserId);
        if (targetUser == null)
        {
            return this.Protocol(Code.NotFound, $"Could not find target user with id `{model.TargetUserId}`!");
        }
        if (currentUser.Id == targetUser.Id)
        {
            return this.Protocol(Code.Conflict, "You can not report yourself!");
        }
        var exists = await dbContext
            .Reports
            .AnyAsync((t) => t.TriggerId == currentUser.Id && t.TargetId == targetUser.Id && t.Status == ReportStatus.Pending);
        if (exists)
        {
            return this.Protocol(Code.NoActionTaken, "You have already reported the target user!");
        }
        // All check passed. Report him now!
        await dbContext.Reports.AddAsync(new Report
        {
            TargetId = targetUser.Id,
            TriggerId = currentUser.Id,
            Reason = model.Reason
        });
        await dbContext.SaveChangesAsync();
        return this.Protocol(Code.JobDone, "Successfully reported target user!");
    }
    
}

