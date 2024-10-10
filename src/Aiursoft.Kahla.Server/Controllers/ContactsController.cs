using Aiursoft.AiurProtocol.Models;
using Aiursoft.AiurProtocol.Server;
using Aiursoft.AiurProtocol.Server.Attributes;
using Aiursoft.DocGenerator.Attributes;
using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Models.AddressModels;
using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.SDK.Models.ViewModels;
using Aiursoft.Kahla.Server.Attributes;
using Aiursoft.Kahla.Server.Data;
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
    ILogger<ContactsController> logger,
    KahlaThreadMapper kahlaThreadMapper,
    KahlaUserMapper kahlaUserMapper,
    KahlaDbContext dbContext,
    UserManager<KahlaUser> userManager) : ControllerBase
{
    // This lock is used to prevent adding the same friend twice.
    private static readonly SemaphoreSlim AddFriendLock = new(1, 1);

    [HttpGet]
    [Route("mine")]
    public async Task<IActionResult> Mine()
    {
        var user = await this.GetCurrentUser(userManager);
        logger.LogInformation("User with email: {Email} is trying to get all his known contacts.", user.Email);
        await dbContext.Entry(user).Collection(t => t.KnownContacts).LoadAsync();
        var knownContacts = user.KnownContacts
            .Select(t => t.Target)
            .OrderBy(t => t?.NickName)
            .Select(kahlaUserMapper.MapOthersView)
            .ToList();
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
        
        var usersQuery = dbContext
            .Users
            .AsNoTracking()
            .Where(t => t.AllowSearchByName || t.Id == model.SearchInput)
            .Where(t =>
                t.Email.Contains(model.SearchInput) ||
                t.NickName.Contains(model.SearchInput) ||
                t.Id == model.SearchInput);
        var usersEntities = await usersQuery
            .Take(model.Take)
            .ToListAsync();
        var usersView = usersEntities
            .Select(kahlaUserMapper.MapOthersView)
            .ToList();
        logger.LogInformation("User with email: {Email} successfully get {Count} users.", user.Email, usersView.Count);
        
        var threadsQuery = dbContext
            .ChatThreads
            .AsNoTracking()
            .Where(t => t.AllowSearchByName || t.Id.ToString() == model.SearchInput)
            .Where(t => 
                t.Name.Contains(model.SearchInput) ||
                t.Id.ToString() == model.SearchInput);
        var threadsEntities = await threadsQuery
            .Take(model.Take)
            .ToListAsync();
        var threadsView = threadsEntities
            .Select(kahlaThreadMapper.MapSearchedThread)
            .ToList();
        logger.LogInformation("User with email: {Email} successfully get {Count} threads.", user.Email, threadsView.Count);
    
        return this.Protocol(new SearchEverythingViewModel
        {
            TotalUsersCount = await usersQuery.CountAsync(),
            TotalThreadsCount = await threadsQuery.CountAsync(),
            Users = usersView,
            Threads = threadsView,
            Code = Code.ResultShown,
            Message = "Search result is shown."
        });
    }

    [HttpGet]
    [Route("details/{id}")]
    public async Task<IActionResult> Details([FromRoute]string id)
    {
        var user = await this.GetCurrentUser(userManager);
        logger.LogInformation("User with email: {Email} is trying to download the detailed info with a contact with id: {TargetId}.", user.Email, id);
        var target = await dbContext.Users.FindAsync(id);
        if (target == null)
        {
            logger.LogWarning("User with email: {Email} is trying to download the detailed info with a contact with id: {TargetId} but the target does not exist.", user.Email, id);
            return this.Protocol(Code.NotFound, "The target user does not exist.");
        }
        var mapped = await kahlaUserMapper.MapDetailedOthersView(target, user);
        logger.LogInformation("User with email: {Email} successfully downloaded the detailed info with a contact with id: {TargetId}.", user.Email, id);
        return this.Protocol(new UserDetailViewModel 
        {
            User = mapped,
            Code = Code.ResultShown,
            Message = "User detail is shown."
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
}

