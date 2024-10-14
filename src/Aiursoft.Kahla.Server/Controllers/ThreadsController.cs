using Aiursoft.AiurProtocol.Models;
using Aiursoft.AiurProtocol.Server;
using Aiursoft.AiurProtocol.Server.Attributes;
using Aiursoft.DocGenerator.Attributes;
using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Models.AddressModels;
using Aiursoft.Kahla.SDK.Models.Entities;
using Aiursoft.Kahla.SDK.Models.Mapped;
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
[Route("api/threads")]
public class ThreadsController(
    ILogger<ThreadsController> logger,
    ThreadJoinedViewAppService threadService,
    UserInThreadViewAppService userAppService,
    KahlaDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [Route("list")]
    [Produces<MyThreadsViewModel>]
    public async Task<IActionResult> Search([FromQuery]SearchAddressModel model)
    {
        var currentUserId = User.GetUserId();
        logger.LogInformation("User with Id: {Id} is trying to search his threads with keyword: {Search}.", currentUserId, model.SearchInput);
        var (count, threads) = await threadService.SearchThreadsIJoinedAsync(model.SearchInput, model.Excluding, currentUserId, model.Skip, model.Take);
        logger.LogInformation("User with Id: {Id} successfully searched his threads with keyword: {Search} with total {Count}.", currentUserId, model.SearchInput, threads.Count);
        return this.Protocol(new MyThreadsViewModel
        {
            Code = Code.ResultShown,
            Message = $"Successfully get your first {model.Take} threads from search result and skipped {model.Skip} threads.",
            KnownThreads = threads,
            TotalCount = count
        });
    }
    
    [HttpGet]
    [Route("members/{id:int}")]
    [Produces<ThreadMembersViewModel>]
    public async Task<IActionResult> Members([FromRoute]int id, [FromQuery]int skip = 0, [FromQuery]int take = 20)
    {
        var currentUserId = User.GetUserId();
        var myRelation = await dbContext.UserThreadRelations
            .Where(t => t.UserId == currentUserId)
            .Where(t => t.ThreadId == id)
            .Include(t => t.Thread)
            .FirstOrDefaultAsync();
        if (myRelation == null)
        {
            return this.Protocol(Code.Unauthorized, "You are not a member of this thread.");
        }
        if (myRelation.Thread.AllowMembersEnlistAllMembers == false && myRelation.UserThreadRole != UserThreadRole.Admin)
        {
            return this.Protocol(Code.Unauthorized, "This thread does not allow members to enlist members.");
        }
        var (count, members) = await userAppService.QueryMembersInThreadAsync(id, currentUserId, skip, take);
        return this.Protocol(new ThreadMembersViewModel
        {
            Code = Code.ResultShown,
            Message = $"Successfully get the first {take} members of the thread and skipped {skip} members.",
            Members = members,
            TotalCount = count
        });
    }

    [HttpGet]
    [Route("details/{id:int}")]
    public async Task<IActionResult> Details([FromRoute] int id)
    {
        var currentUserId = User.GetUserId();
        var myRelation = await dbContext.UserThreadRelations
            .Where(t => t.UserId == currentUserId)
            .Where(t => t.ThreadId == id)
            .Include(t => t.Thread)
            .FirstOrDefaultAsync();
        if (myRelation == null)
        {
            return this.Protocol(Code.Unauthorized, "You are not a member of this thread.");
        }
        var thread = await threadService.GetThreadAsync(id, currentUserId);
        if (thread == null)
        {
            return this.Protocol(Code.NotFound, "The thread does not exist.");
        }
        return this.Protocol(new ThreadDetailsViewModel
        {
            Code = Code.ResultShown,
            Message = "Successfully get the thread details.",
            Thread = thread,
        });
    }
    
    [HttpPost]
    [Route("create-scratch")]
    public async Task<IActionResult> CreateFromScratch([FromForm]CreateThreadAddressModel model)
    {
        var currentUserId = User.GetUserId();
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                // Step 1: Create a new thread without setting OwnerRelationId initially.
                var thread = new ChatThread
                {
                    Name = model.Name,
                    AllowSearchByName = model.AllowSearchByName,
                    AllowDirectJoinWithoutInvitation = model.AllowDirectJoinWithoutInvitation,
                    AllowMemberSoftInvitation = model.AllowMemberSoftInvitation,
                    AllowMembersSendMessages = model.AllowMembersSendMessages,
                    AllowMembersEnlistAllMembers = model.AllowMembersEnlistAllMembers
                };
                dbContext.ChatThreads.Add(thread);
                await dbContext.SaveChangesAsync();
                
                // Step 2: Add myself to the thread with role Admin
                var myRelation = new UserThreadRelation
                {
                    UserId = currentUserId,
                    ThreadId = thread.Id,
                    UserThreadRole = UserThreadRole.Admin
                };
                dbContext.UserThreadRelations.Add(myRelation);
                await dbContext.SaveChangesAsync();
                
                // Step 3: Set the owner of the thread after myRelation is saved
                thread.OwnerRelationId = myRelation.Id;
                await dbContext.SaveChangesAsync();
                
                // Commit the transaction if everything is successful
                await transaction.CommitAsync();
                return this.Protocol(new CreateNewThreadViewModel
                {
                    NewThreadId = thread.Id,
                    Code = Code.JobDone,
                    Message = "The thread has been created successfully."
                });
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to create a thread.");
                await transaction.RollbackAsync();
                return this.Protocol(Code.UnknownError,
                    "Failed to create the thread. Might because of a database error.");
            }
        });
    }
        
    [HttpPost]
    [Route("hard-invite/{id}")]
    [Produces<CreateNewThreadViewModel>]
    public async Task<IActionResult> HardInvite([FromRoute]string id)
    {
        var currentUserId = User.GetUserId();
        var targetUser = await dbContext.Users.FindAsync(id);
        if (targetUser == null)
        {
            return this.Protocol(Code.NotFound, $"The target user with ID {id} does not exist.");
        }
        if (!targetUser.AllowHardInvitation)
        {
            return this.Protocol(Code.Unauthorized, "The target user does not allow hard invitation.");
        }
        var targetUserBlockedMe = await dbContext.BlockRecords
            .Where(t => t.CreatorId == targetUser.Id)
            .Where(t => t.TargetId == currentUserId)
            .AnyAsync();
        if (targetUserBlockedMe)
        {
            return this.Protocol(Code.Conflict, "The target user has blocked you so you can not create a thread with him/her.");
        }
        
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                // Step 1: Create a new thread without setting OwnerRelationId initially.
                var thread = new ChatThread();
                dbContext.ChatThreads.Add(thread);
                logger.LogInformation("Creating a new thread...");
                await dbContext.SaveChangesAsync(); // This will generate the thread's id

                // Step 2: Add myself to the thread with role Admin
                var myRelation = new UserThreadRelation
                {
                    UserId = currentUserId,
                    ThreadId = thread.Id,
                    UserThreadRole = UserThreadRole.Admin
                };
                dbContext.UserThreadRelations.Add(myRelation);
                logger.LogInformation("Adding myself (ID is {ID}) to the thread...", currentUserId);
                await dbContext.SaveChangesAsync(); // This will generate myRelation's id

                // Step 3: Set the owner of the thread after myRelation is saved
                thread.OwnerRelationId = myRelation.Id;
                dbContext.ChatThreads.Update(thread);
                // Don't call SaveChangesAsync here for better performance.

                if (currentUserId != targetUser.Id)
                {
                    // Step 4: Add the target user to the thread
                    var targetRelation = new UserThreadRelation
                    {
                        UserId = targetUser.Id,
                        ThreadId = thread.Id,
                        UserThreadRole = UserThreadRole.Member
                    };
                    dbContext.UserThreadRelations.Add(targetRelation);
                    // Don't call SaveChangesAsync here for better performance.
                }
                else
                {
                    logger.LogWarning(
                        "The current user and the target user are the same. Skip adding the target user to the thread. This might because of the user creating a thread with himself/herself.");
                }

                // Commit the transaction if everything is successful
                logger.LogInformation("Setting the owner of the thread to myself (ID is {ID})... And adding the target user (ID is {ID}) to the thread...", currentUserId, targetUser.Id);
                await dbContext.SaveChangesAsync(); // Save the targetRelation
                
                await transaction.CommitAsync();
                logger.LogInformation("The thread has been created successfully.");
                return this.Protocol(new CreateNewThreadViewModel
                {
                    NewThreadId = thread.Id,
                    Code = Code.JobDone,
                    Message = "The thread has been created successfully."
                });
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to create a thread.");
                await transaction.RollbackAsync();
                return this.Protocol(Code.UnknownError,
                    "Failed to create the thread. Might because of a database error.");
            }
        });
    }
}

public class ThreadDetailsViewModel : AiurResponse
{
    public required KahlaThreadMappedJoinedView? Thread { get; set; }
}
